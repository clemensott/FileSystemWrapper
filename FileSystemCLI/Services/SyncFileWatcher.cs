using FileSystemCLI.Models;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files.Change;
using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.Linq.DataStructures;

namespace FileSystemCLI.Services;

public class SyncFileWatcher
{
    private readonly TimeSpan syncDelay = TimeSpan.FromSeconds(5);

    private readonly bool isTestRun;
    private readonly string localFolderPath, serverFolderPath;
    private readonly SyncPairModel syncPair;
    private readonly Api api;
    private readonly FileSystemWatcher fileSystemWatcher;
    private readonly LockQueue<string> changedFiles = new LockQueue<string>();
    private readonly HashSet<string> backlog = new HashSet<string>();
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly SemaphoreSlim syncSem;

    private bool isEnabled;
    private Task? syncTask, watchServerTask, fullSyncTask;
    private DateTime? lastServerChangeFetch;
    private SyncPairState? lastState;

    public SyncFileWatcher(bool isTestRun, SyncPairModel syncPair, Api api, SemaphoreSlim syncSem,
        SyncPairState? lastState = null)
    {
        this.isTestRun = isTestRun;
        this.syncPair = syncPair;
        this.api = api;
        this.syncSem = syncSem;
        this.lastState = lastState;

        localFolderPath = Path.GetFullPath(syncPair.LocalFolderPath);
        serverFolderPath = syncPair.ServerFolderPath;

        fileSystemWatcher = new FileSystemWatcher(localFolderPath);
        fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Size;
        fileSystemWatcher.IncludeSubdirectories = syncPair.WithSubfolders;

        fileSystemWatcher.Changed += OnChanged;
        fileSystemWatcher.Created += OnChanged;
        fileSystemWatcher.Deleted += OnChanged;
        fileSystemWatcher.Renamed += OnRenamed;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            changedFiles.Enqueue(Path.GetRelativePath(localFolderPath, e.FullPath));
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Error on file changed: {exc.Message}");
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            changedFiles.Enqueue(new string[]
            {
                Path.GetRelativePath(localFolderPath, e.OldFullPath),
                Path.GetRelativePath(localFolderPath, e.FullPath),
            });
        }
        catch (Exception exc)
        {
            Console.WriteLine($"Error on file renamed: {exc.Message}");
        }
    }

    private async Task<SyncPairState> GetLastSyncPairState()
    {
        return lastState ??= await SyncPairState.LoadSyncPairState(syncPair.StateFilePath);
    }

    public void Start()
    {
        if (isEnabled) return;
        isEnabled = true;

        if (syncPair.Mode is SyncMode.LocalToServer or SyncMode.LocalToServerCreateOnly or SyncMode.TwoWay)
        {
            fileSystemWatcher.EnableRaisingEvents = true;
            Console.WriteLine($"Watching local: {localFolderPath}");
        }

        if (syncPair.Mode is SyncMode.ServerToLocal or SyncMode.ServerToLocalCreateOnly or SyncMode.TwoWay)
        {
            watchServerTask = Task.Run(CheckServerChanges);
        }

        syncTask = Task.Run(CheckChangedFiles);
        fullSyncTask = Task.Run(CheckFullSync);
    }

    private async Task CheckServerChanges()
    {
        Console.WriteLine($"Watching server: {serverFolderPath}");
        while (isEnabled)
        {
            try
            {
                SyncPairState state = await GetLastSyncPairState();
                DateTime since = lastServerChangeFetch ?? state.LastServerChangeSync;

                await api.Ensure();
                List<FileChangeInfo> fileChanges =
                    await api.GetAllFileChanges(serverFolderPath, since);

                lastServerChangeFetch = DateTime.UtcNow;

                if (fileChanges.Count > 0)
                {
                    changedFiles.Enqueue(fileChanges.Select(fc => fc.RelativePath).ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching server changes: {e.Message}");
            }

            try
            {
                await Task.Delay(syncPair.ServerFetchChangesInterval, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task CheckChangedFiles()
    {
        while (isEnabled)
        {
            if (backlog.Count == 0 || changedFiles.Count > 0)
            {
                (_, string[]? files) = changedFiles.DequeueBatch();
                if (!isEnabled) break;

                foreach (string file in files) backlog.Add(file);
            }

            try
            {
                // Current batch includes all server changes
                // and therefore the current lastServerChangeFetch can be saved in state
                DateTime? serverChangeFetch = lastServerChangeFetch;

                HashSet<string> set = new HashSet<string>(backlog);
                Console.WriteLine($"Changed {set.Count} files...");

                try
                {
                    // Wait for files to not be changed in last few seconds (syncDelay)
                    await Task.Delay(syncDelay, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                string[] delayFiles = changedFiles.ToArray();
                // Don't sync files that changed in last few seconds (syncDelay)
                foreach (string delayFile in delayFiles) set.Remove(delayFile);

                if (set.Count > 0) await SyncFiles(set.ToArray(), serverChangeFetch);
                backlog.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error syncing files: {e.Message}");
            }
        }
    }

    private async Task SyncFiles(string[] relativeFilePaths, DateTime? serverChangeFetch)
    {
        try
        {
            await syncSem.WaitAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            await api.Ensure();

            Console.WriteLine($"Syncing {relativeFilePaths.Length} files...");
            IEnumerable<FilePairModel> filePairs = relativeFilePaths.Select(relativeFilePath => new FilePairModel()
            {
                RelativePath = relativeFilePath,
                LocalFilePath = Path.Combine(localFolderPath, relativeFilePath),
                ServerFullPath = api.Config.JoinPaths(syncPair.ServerFolderPath, relativeFilePath),
            }).ToArray();

            SyncPairState state = await GetLastSyncPairState();
            SyncPairHandler handler = new SyncPairHandler(isTestRun, syncPair, api, state);
            await handler.Run(filePairs);

            HashSet<string> relativeFilePathsSet = new HashSet<string>(filePairs.Select(f => f.RelativePath));

            IEnumerable<SyncPairStateFileModel> currentStateFiles = state
                .Where(f => !relativeFilePathsSet.Contains(f.RelativePath))
                .Concat(handler.CurrentState);

            lastState = new SyncPairState(currentStateFiles)
            {
                LastFullSync = state.LastFullSync,
                LastServerChangeSync = serverChangeFetch ?? state.LastServerChangeSync,
            };

            if (!isTestRun)
            {
                await lastState.WriteSyncPairState(syncPair.StateFilePath);
            }

            Console.WriteLine($"Synced files!!");
        }
        finally
        {
            syncSem.Release();
        }
    }

    private async Task CheckFullSync()
    {
        while (isEnabled)
        {
            SyncPairState state = await GetLastSyncPairState();

            TimeSpan timeUntilNextFullSync = (state.LastFullSync + syncPair.FullSyncInterval) - DateTime.UtcNow;
            if (timeUntilNextFullSync < TimeSpan.Zero)
            {
                await FullSync(state);
                continue;
            }

            try
            {
                await Task.Delay(timeUntilNextFullSync, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task FullSync(SyncPairState state)
    {
        try
        {
            await syncSem.WaitAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            await api.Ensure();

            SyncPairHandler handler = new SyncPairHandler(isTestRun, syncPair, api, state);
            await handler.Run();

            lastState = handler.CurrentState;
            if (!isTestRun)
            {
                await lastState.WriteSyncPairState(syncPair.StateFilePath);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error full syncing files: {e.Message}");
        }
        finally
        {
            syncSem.Release();
        }
    }

    public void Stop()
    {
        isEnabled = false;
        fileSystemWatcher.EnableRaisingEvents = false;

        changedFiles.End();
        cancellationTokenSource.Cancel();
    }

    public async Task AwaitSyncTask()
    {
        if (watchServerTask != null) await watchServerTask;
        if (syncTask != null) await syncTask;
        if (fullSyncTask != null) await fullSyncTask;
    }
}
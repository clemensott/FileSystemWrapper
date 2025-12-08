using FileSystemCLI.Models;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files.Change;
using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.Linq.DataStructures;

namespace FileSystemCLI.Services;

public class SyncFileWatcher
{
    private readonly TimeSpan apiInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan syncDelay = TimeSpan.FromSeconds(5);

    private readonly bool isTestRun;
    private readonly string localFolderPath, serverFolderPath;
    private readonly SyncPairModel syncPair;
    private readonly Api api;
    private readonly FileSystemWatcher fileSystemWatcher;
    private readonly LockQueue<string> changedFiles = new LockQueue<string>();

    private bool isEnabled;
    private Task? syncTask, watchServerTask;
    private DateTime? lastServerChangeFetch;
    private SyncPairState? lastState;

    public SyncFileWatcher(bool isTestRun, SyncPairModel syncPair, Api api, SyncPairState? lastState = null)
    {
        this.isTestRun = isTestRun;
        this.syncPair = syncPair;
        this.api = api;
        this.lastState = lastState;

        localFolderPath = Path.GetFullPath(syncPair.LocalFolderPath);
        serverFolderPath = api.Config.JoinPaths(syncPair.ServerFolderPath);

        fileSystemWatcher = new FileSystemWatcher(localFolderPath);
        fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Size;
        fileSystemWatcher.IncludeSubdirectories = syncPair.WithSubfolders;

        fileSystemWatcher.Changed += OnChanged;
        fileSystemWatcher.Created += OnChanged;
        fileSystemWatcher.Deleted += OnChanged;
        fileSystemWatcher.Renamed += OnChanged;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        changedFiles.Enqueue(Path.GetRelativePath(localFolderPath, e.FullPath));
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
            watchServerTask = Task.Run(async () =>
            {
                Console.WriteLine($"Watching server: {serverFolderPath}");
                while (isEnabled)
                {
                    try
                    {
                        SyncPairState state = await GetLastSyncPairState();
                        DateTime since = lastServerChangeFetch ?? state.LastServerChangeSync;

                        List<FileChangeInfo> fileChanges =
                            await api.GetAllFileChanges(serverFolderPath, since);

                        lastServerChangeFetch = DateTime.Now;

                        if (fileChanges.Count > 0)
                        {
                            changedFiles.Enqueue(fileChanges.Select(fc => fc.RelativePath).ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error fetching server changes: {e.Message}");
                    }

                    await Task.Delay(apiInterval);
                }
            });
        }

        syncTask = Task.Run(async () =>
        {
            while (isEnabled)
            {
                (bool isEnd, string[]? files) = changedFiles.DequeueBatch();
                if (isEnd) break;

                // Current batch includes all server changes
                // and therefore the current lastServerChangeFetch can be saved in state
                DateTime? serverChangeFetch = lastServerChangeFetch;

                HashSet<string> set = new HashSet<string>(files);

                // Wait for files to not be changed in last few seconds (syncDelay)
                await Task.Delay(syncDelay);
                string[] delayFiles = changedFiles.ToArray();
                // Don't sync files that changed in last few seconds (syncDelay)
                foreach (string delayFile in delayFiles) set.Remove(delayFile);

                if (set.Count > 0) await SyncFiles(set.ToArray(), serverChangeFetch);
            }
        });
    }

    private async Task SyncFiles(string[] relativeFilePaths, DateTime? serverChangeFetch)
    {
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
            .Where(f => relativeFilePathsSet.Contains(f.RelativePath))
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

    public void Stop()
    {
        isEnabled = false;
        fileSystemWatcher.EnableRaisingEvents = false;

        changedFiles.End();
    }

    public async Task AwaitSyncTask()
    {
        if (watchServerTask != null) await watchServerTask;
        if (syncTask != null) await syncTask;
    }
}
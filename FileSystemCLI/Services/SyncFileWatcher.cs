using FileSystemCLI.Models;
using FileSystemCommon;
using StdOttStandard.Linq.DataStructures;

namespace FileSystemCLI.Services;

public class SyncFileWatcher
{
    private readonly TimeSpan apiInterval = TimeSpan.FromMinutes(1);
    private readonly TimeSpan syncDelay = TimeSpan.FromSeconds(5);

    private readonly bool isTestRun;
    private readonly string syncFolderPath;
    private readonly SyncPairModel syncPair;
    private readonly Api api;
    private readonly FileSystemWatcher fileSystemWatcher;
    private readonly LockQueue<string> changedFiles = new LockQueue<string>();

    private bool isEnabled;
    private Task? syncTask;
    private SyncPairState? lastState;

    public SyncFileWatcher(bool isTestRun, SyncPairModel syncPair, Api api, SyncPairState? lastState = null)
    {
        this.isTestRun = isTestRun;
        this.syncPair = syncPair;
        this.api = api;
        this.lastState = lastState;

        syncFolderPath = Path.GetFullPath(syncPair.LocalFolderPath);

        fileSystemWatcher = new FileSystemWatcher(syncFolderPath);
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
        changedFiles.Enqueue(e.FullPath);
    }

    private async Task<SyncPairState> GetLastSyncPairState()
    {
        lastState ??= await SyncPairState.LoadSyncPairState(syncPair.StateFilePath);
        ;

        return lastState;
    }

    public void Start()
    {
        if (isEnabled) return;

        fileSystemWatcher.EnableRaisingEvents = true;
        isEnabled = true;

        syncTask = Task.Run(async () =>
        {
            Console.WriteLine($"Watching: {syncFolderPath}");
            while (isEnabled)
            {
                (bool isEnd, string[]? files) = changedFiles.DequeueBatch();
                if (isEnd) break;

                HashSet<string> set = new HashSet<string>(files);
                Console.WriteLine($"Files changed: {string.Join(',', set)}");

                // Wait for files to not be changed in last few seconds (syncDelay)
                await Task.Delay(syncDelay);
                string[] delayFiles = changedFiles.ToArray();
                Console.WriteLine($"Delay files: {string.Join(',', delayFiles)}");
                // Don't sync files that changed in last few seconds (syncDelay)
                foreach (string delayFile in delayFiles) set.Remove(delayFile);

                Console.WriteLine($"Syncing files: {string.Join(',', set)}");
                if (set.Count > 0) await SyncFiles(set.ToArray());
            }
        });
    }

    private async Task SyncFiles(string[] fullFilePaths)
    {
        Console.WriteLine($"Syncing {fullFilePaths.Length} files...");
        IEnumerable<FilePairModel> filePairs = fullFilePaths.Select(localFilePath =>
        {
            string relativePath = Path.GetRelativePath(syncFolderPath, localFilePath);
            return new FilePairModel()
            {
                RelativePath = relativePath,
                LocalFilePath = localFilePath,
                ServerFullPath = api.Config.JoinPaths(syncPair.ServerFolderPath, relativePath),
            };
        }).ToArray();

        SyncPairState state = await GetLastSyncPairState();
        SyncPairHandler handler = new SyncPairHandler(isTestRun, syncPair, api, state);
        await handler.Run(filePairs);

        HashSet<string> relativeFilePaths = new HashSet<string>(filePairs.Select(f => f.RelativePath));

        IEnumerable<SyncPairStateFileModel> currentStateFiles = state
            .Where(f => relativeFilePaths.Contains(f.RelativePath))
            .Concat(handler.CurrentState);

        lastState = new SyncPairState(currentStateFiles);

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
        if (syncTask != null) await syncTask;
    }
}
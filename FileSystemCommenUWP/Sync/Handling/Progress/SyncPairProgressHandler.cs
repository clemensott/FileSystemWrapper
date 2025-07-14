using FileSystemCommonUWP.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Progress
{
    public class SyncPairProgressHandler
    {
        private readonly int syncPairRunId;
        private readonly AppDatabase database;
        private readonly SemaphoreSlim updateSlim;
        private readonly LatestValue<SyncPairHandlerState> latestState;
        private readonly LatestValue<int> totalFilesCount;
        private readonly Queue<SyncPairRunFile> insertFileQueue;
        private readonly HashSet<string> insertedFiles;
        private readonly Queue<SyncPairProgressFileUpdate> updateFileQueue;
        private readonly Queue<SyncPairProgressErrorFileUpdate> updateErrorFileQueue;
        private readonly LatestValue<string> latestCurrentQueryFolderRelPath, latestCurrentCopyToLocalRelPath,
            latestCurrentCopyToServerRelPath, latestCurrentDeleteFromServerRelPath, latestCurrentDeleteFromLocalRelPath;

        private bool isEnded;
        private Task task;

        public event EventHandler Progress;

        public SyncPairProgressHandler(int syncPairRunId, AppDatabase database)
        {
            this.syncPairRunId = syncPairRunId;
            this.database = database;
            updateSlim = new SemaphoreSlim(0);
            latestState = new LatestValue<SyncPairHandlerState>();
            totalFilesCount = new LatestValue<int>(0);
            insertFileQueue = new Queue<SyncPairRunFile>();
            insertedFiles = new HashSet<string>();
            updateFileQueue = new Queue<SyncPairProgressFileUpdate>();
            updateErrorFileQueue = new Queue<SyncPairProgressErrorFileUpdate>();
            latestCurrentQueryFolderRelPath = new LatestValue<string>();
            latestCurrentCopyToLocalRelPath = new LatestValue<string>();
            latestCurrentCopyToServerRelPath = new LatestValue<string>();
            latestCurrentDeleteFromServerRelPath = new LatestValue<string>();
            latestCurrentDeleteFromLocalRelPath = new LatestValue<string>();
        }

        public void Start()
        {
            task = Task.Run(Run);
        }

        public async Task End()
        {
            isEnded = true;
            updateSlim.Release();
            await task;
        }

        private async Task Run()
        {
            while (!isEnded)
            {
                do
                {
                    await updateSlim.WaitAsync();
                } while (updateSlim.CurrentCount > 0);

                await Check();
            }

            await Check();
        }

        private async Task Check()
        {
            try
            {
                await CheckState();
                await CheckTotalFilesCount();
                await CheckInsertFiles();
                await CheckUpateFiles();
                await CheckUpdateErrorFiles();
                await CheckRelPaths();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"SyncPairProgressHandler.Check error: {e}");
            }
        }

        private async Task CheckState()
        {
            SyncPairHandlerState state;
            if (latestState.TryGetNewValue(out state))
            {
                await database.SyncPairs.UpdateSyncPairRunState(syncPairRunId, state);
                Progress?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetState(SyncPairHandlerState state)
        {
            latestState.SetValue(state);
            updateSlim.Release();
        }

        private async Task CheckTotalFilesCount()
        {
            int totalFilesCount;
            if (this.totalFilesCount.TryGetNewValue(out totalFilesCount))
            {
                await database.SyncPairs.UpdateSyncPairRunAllFilesCount(syncPairRunId, totalFilesCount);
                Progress?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task CheckInsertFiles()
        {
            const int maxBatchSize = 367;

            List<SyncPairRunFile> batch;
            lock (insertFileQueue)
            {
                if (insertFileQueue.Count == 0) return;

                batch = new List<SyncPairRunFile>();
                while (insertFileQueue.Count > 0 && batch.Count < maxBatchSize)
                {
                    batch.Add(insertFileQueue.Dequeue());
                }
            }

            await database.SyncPairs.InsertSyncPairRunFiles(syncPairRunId, batch);
            Progress?.Invoke(this, EventArgs.Empty);

            foreach (string relativePath in batch.Select(f => f.RelativePath))
            {
                insertedFiles.Add(relativePath);
            }
        }

        public void AddFileToAll(SyncPairRunFile file)
        {
            lock (insertFileQueue)
            {
                insertFileQueue.Enqueue(file);
            }
            totalFilesCount.SetValue(count => count + 1);
            updateSlim.Release();
        }

        private async Task CheckUpateFiles()
        {
            const int maxBatchSize = 713;
            List<SyncPairProgressFileUpdate> batch;
            lock (updateFileQueue)
            {
                if (updateFileQueue.Count == 0) return;

                batch = new List<SyncPairProgressFileUpdate>();
                while (updateFileQueue.Count > 0 && batch.Count < maxBatchSize)
                {
                    string peekRelativePath = updateFileQueue.Peek().RelativePath;
                    if (!insertedFiles.Contains(peekRelativePath)) break;

                    batch.Add(updateFileQueue.Dequeue());
                }
            }

            await database.SyncPairs.SetSyncPairRunFileTypes(syncPairRunId, batch);
            Progress?.Invoke(this, EventArgs.Empty);
        }

        public void AddFileToList(FilePair pair, SyncPairRunFileType type, bool increaseCurrentCount)
        {
            lock (updateFileQueue)
            {
                updateFileQueue.Enqueue(new SyncPairProgressFileUpdate(pair.RelativePath, type, increaseCurrentCount));
            }
            updateSlim.Release();
        }

        public async Task CheckUpdateErrorFiles()
        {
            const int maxBatchSize = 213;
            List<SyncPairProgressErrorFileUpdate> batch;
            lock (updateErrorFileQueue)
            {
                if (updateErrorFileQueue.Count == 0) return;

                batch = new List<SyncPairProgressErrorFileUpdate>();
                while (updateErrorFileQueue.Count > 0 && batch.Count < maxBatchSize)
                {
                    string peekRelativePath = updateErrorFileQueue.Peek().RelativePath;
                    if (!insertedFiles.Contains(peekRelativePath)) break;

                    batch.Add(updateErrorFileQueue.Dequeue());
                }
            }

            if (batch.Count == 0) return;

            await database.SyncPairs.SetSyncPairRunErrorFileTypes(syncPairRunId, batch);
            Progress?.Invoke(this, EventArgs.Empty);
        }

        public void AddFileToErrorList(FilePair pair, string message, Exception e = null)
        {
            lock (updateErrorFileQueue)
            {
                updateErrorFileQueue.Enqueue(new SyncPairProgressErrorFileUpdate(pair.RelativePath, message, e));
            }
            updateSlim.Release();
        }

        private async Task CheckRelPaths()
        {
            string relPath;
            if (latestCurrentQueryFolderRelPath.TryGetNewValue(out relPath))
            {
                await database.SyncPairs.UpdateSyncPairRunCurrentQueryFolderRelPath(syncPairRunId, relPath);
                Progress?.Invoke(this, EventArgs.Empty);
            }
            if (latestCurrentCopyToLocalRelPath.TryGetNewValue(out relPath))
            {
                await database.SyncPairs.UpdateSyncPairRunCurrentCopyToLocalRelPath(syncPairRunId, relPath);
                Progress?.Invoke(this, EventArgs.Empty);
            }
            if (latestCurrentCopyToServerRelPath.TryGetNewValue(out relPath))
            {
                await database.SyncPairs.UpdateSyncPairRunCurrentCopyToServerRelPath(syncPairRunId, relPath);
                Progress?.Invoke(this, EventArgs.Empty);
            }
            if (latestCurrentDeleteFromServerRelPath.TryGetNewValue(out relPath))
            {
                await database.SyncPairs.UpdateSyncPairRunCurrentDeleteFromServerRelPath(syncPairRunId, relPath);
                Progress?.Invoke(this, EventArgs.Empty);
            }
            if (latestCurrentDeleteFromLocalRelPath.TryGetNewValue(out relPath))
            {
                await database.SyncPairs.UpdateSyncPairRunCurrentCopyToLocalRelPath(syncPairRunId, relPath);
                Progress?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateCurrentQueryFolderRelPath(string relPath)
        {
            latestCurrentQueryFolderRelPath.SetValue(relPath);
            updateSlim.Release();
        }

        public void UpdateCurrentCopyToLocalRelPath(string relPath)
        {
            latestCurrentCopyToLocalRelPath.SetValue(relPath);
            updateSlim.Release();
        }

        public void UpdateCurrentCopyToServerRelPath(string relPath)
        {
            latestCurrentCopyToServerRelPath.SetValue(relPath);
            updateSlim.Release();
        }

        public void UpdateCurrentDeleteFromServerRelPath(string relPath)
        {
            latestCurrentDeleteFromServerRelPath.SetValue(relPath);
            updateSlim.Release();
        }

        public void UpdateCurrentDeleteFromLocalRelPath(string relPath)
        {
            latestCurrentDeleteFromLocalRelPath.SetValue(relPath);
            updateSlim.Release();
        }
    }
}

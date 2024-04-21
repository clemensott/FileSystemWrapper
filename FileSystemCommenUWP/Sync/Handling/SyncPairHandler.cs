using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using FileSystemCommonUWP.Sync.Handling.Mode;
using StdOttStandard.Linq;
using StdOttStandard.Linq.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class SyncPairHandler
    {
        private const int partialHashSize = 10 * 1024; // 10 kB

        private readonly AppDatabase database;

        private readonly bool withSubfolders, isTestRun, requestedCancel;
        private readonly string serverPath;
        private readonly StorageFolder localFolder;
        private readonly SyncModeHandler modeHandler;
        private readonly ISyncFileComparer fileComparer;
        private readonly SyncConflictHandlingType conlictHandlingType;
        private readonly string[] allowList, denialList;
        private readonly Api api;
        private readonly SyncPairResult lastResult, newResult;

        private SyncPairHandlerState state;
        private readonly LockQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;
        private readonly LockQueue<Func<Task>> dbDatabaseProgressSteps;

        public int SyncPairRunId { get; }

        public string RunToken { get; }

        public bool IsCanceled => state == SyncPairHandlerState.Canceled;

        public bool IsEnded => state == SyncPairHandlerState.Finished ||
            state == SyncPairHandlerState.Error ||
            state == SyncPairHandlerState.Canceled;

        public event EventHandler Progress;

        public SyncPairHandler(AppDatabase database, int syncPairRunId, bool withSubfolders, bool isTestRun, bool requestedCancel,
            SyncPairResult lastResult, SyncMode mode, SyncCompareType compareType, SyncConflictHandlingType conflictHandlingType,
            StorageFolder localFolder, string serverPath, string[] allowList, string[] denialList, Api api)
        {
            bothFiles = new LockQueue<FilePair>();
            singleFiles = new LockQueue<FilePair>();
            copyToLocalFiles = new LockQueue<FilePair>();
            copyToServerFiles = new LockQueue<FilePair>();
            deleteLocalFiles = new LockQueue<FilePair>();
            deleteSeverFiles = new LockQueue<FilePair>();
            dbDatabaseProgressSteps = new LockQueue<Func<Task>>();

            this.database = database;
            this.SyncPairRunId = syncPairRunId;
            this.withSubfolders = withSubfolders;
            this.requestedCancel = requestedCancel;
            this.isTestRun = isTestRun;
            fileComparer = GetFileComparer(compareType);
            conlictHandlingType = conflictHandlingType;
            modeHandler = GetSyncModeHandler(mode, fileComparer, lastResult, conflictHandlingType, api);
            this.localFolder = localFolder;
            this.serverPath = serverPath;
            this.allowList = allowList?.ToArray() ?? new string[0];
            this.denialList = denialList?.ToArray() ?? new string[0];
            this.api = api;
            this.lastResult = lastResult;
            newResult = new SyncPairResult();
        }

        private static SyncModeHandler GetSyncModeHandler(SyncMode mode, ISyncFileComparer fileComparer,
            SyncPairResult lastResult, SyncConflictHandlingType conflictHandlingType, Api api)
        {
            switch (mode)
            {
                case SyncMode.ServerToLocalCreateOnly:
                    return new ServerToLocalCreateOnlyModeHandler(fileComparer, conflictHandlingType, api);

                case SyncMode.ServerToLocal:
                    return new ServerToLocalModeHandler(fileComparer, conflictHandlingType, api);

                case SyncMode.LocalToServerCreateOnly:
                    return new LocalToServerCreateOnlyModeHandler(fileComparer, conflictHandlingType, api);

                case SyncMode.LocalToServer:
                    return new LocalToServerModeHandler(fileComparer, conflictHandlingType, api);

                case SyncMode.TwoWay:
                    return new TwoWayModeHandler(fileComparer, lastResult, conflictHandlingType, api);
            }

            throw new ArgumentException("Value not Implemented: " + mode, nameof(mode));
        }

        private static ISyncFileComparer GetFileComparer(SyncCompareType type)
        {
            switch (type)
            {
                case SyncCompareType.Exists:
                    return new ExistsComparer();

                case SyncCompareType.Size:
                    return new SizeComparer();

                case SyncCompareType.Hash:
                    return new HashComparer();

                case SyncCompareType.PartialHash:
                    return new HashComparer(partialHashSize);
            }

            throw new ArgumentException("Value not Implemented:" + type, nameof(type));
        }

        private FilePair CreateFilePair(string serverBasePath, string relPath, StorageFile localFile, bool serverFileExists)
        {
            string relativePath = relPath.Trim(api.Config.DirectorySeparatorChar);
            string serverFullPath = api.Config.JoinPaths(serverBasePath, relPath);
            string name = Path.GetFileName(serverFullPath);
            return new FilePair(name, relativePath, serverFullPath, serverFileExists, localFile);
        }

        private void SetState(SyncPairHandlerState state)
        {
            this.state = state;
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunState(SyncPairRunId, state));
        }

        private void AddFileToAll(SyncPairRunFile file)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.InsertSyncPairRunFile(SyncPairRunId, file));
        }

        private void AddToList(FilePair pair, SyncPairRunFileType type, bool increaseCurrentCount)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.SetSyncPairRunFileType(SyncPairRunId, pair.RelativePath, type, increaseCurrentCount));
        }

        private void ErrorFile(FilePair pair, string message, Exception e = null)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.SetSyncPairRunErrorFileType(SyncPairRunId, pair.RelativePath, e, true));
        }

        private void UpdateCurrentQueryFolderRelPath(string path)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunCurrentQueryFolderRelPath(SyncPairRunId, path));
        }

        private void UpdateCurrentCopyToLocalRelPath(string path)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunCurrentCopyToLocalRelPath(SyncPairRunId, path));
        }

        private void UpdatCurrentCopyToServerRelPath(string path)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunCurrentCopyToServerRelPath(SyncPairRunId, path));
        }

        private void UpdateCurrentDeleteFromServerRelPath(string path)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunCurrentDeleteFromServerRelPath(SyncPairRunId, path));
        }

        private void UpdateCurrentDeleteFromLocalRelPath(string path)
        {
            dbDatabaseProgressSteps.Enqueue(() => database.SyncPairs.UpdateSyncPairRunCurrentDeleteFromLocalRelPath(SyncPairRunId, path));
        }

        private void AddResultFile(FilePair pair)
        {
            if (pair.ServerCompareValue == null || pair.LocalCompareValue == null)
            {
                SyncPairResultFile last;
                if (lastResult.TryGetFile(pair.RelativePath, out last))
                {
                    if (pair.ServerCompareValue == null) pair.ServerCompareValue = last.ServerCompareValue;
                    if (pair.LocalCompareValue == null) pair.LocalCompareValue = last.LocalCompareValue;
                }
            }

            newResult.AddFile(new SyncPairResultFile()
            {
                RelativePath = pair.RelativePath,
                LocalCompareValue = pair.LocalCompareValue,
                ServerCompareValue = pair.ServerCompareValue,
            });
        }

        private async Task SaveNewResult()
        {
            await database.SyncPairs.InsertSyncPairResult(SyncPairRunId, newResult);
        }

        private async Task QueryFiles()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(serverPath) && localFolder != null) await Query(string.Empty, localFolder);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("QueryFiles error: " + e);
                throw;
            }
            finally
            {
                UpdateCurrentQueryFolderRelPath(null);

                bothFiles.End();
                singleFiles.End();
                System.Diagnostics.Debug.WriteLine($"Query ended!!!!!!!");
            }

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                UpdateCurrentQueryFolderRelPath(relPath);

                string serverFolderPath = api.Config.JoinPaths(serverPath, relPath);

                Task<FolderContent> serverFolderContentTask = api.FolderContent(serverFolderPath);
                IAsyncOperation<IReadOnlyList<StorageFile>> localFilesTask = localFolder?.GetFilesAsync();

                FolderContent serverFolderContent = await serverFolderContentTask ?? new FolderContent();
                FileSortItem[] serverFiles = serverFolderContent.Files ?? new FileSortItem[0];
                IDictionary<string, StorageFile> localFiles = localFilesTask != null
                    ? (await localFilesTask).ToDictionary(f => f.Name)
                    : new Dictionary<string, StorageFile>();

                if (IsCanceled) return;

                foreach (FileSortItem serverFile in serverFiles)
                {
                    if (!CheckAllowAndDenyList(serverFile.Path)) continue;

                    string relFilePath = api.Config.JoinPaths(relPath, serverFile.Name);
                    StorageFile localFile;

                    AddFileToAll(new SyncPairRunFile()
                    {
                        Name = serverFile.Name,
                        RelativePath = relFilePath,
                    });

                    if (localFiles.TryGetValue(serverFile.Name, out localFile))
                    {
                        localFiles.Remove(serverFile.Name);
                        bothFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, true));
                    }
                    else singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, null, true));
                }

                foreach (StorageFile localFile in localFiles.Values)
                {
                    string relFilePath = api.Config.JoinPaths(relPath, localFile.Name);
                    string serverFilePath = api.Config.JoinPaths(serverPath, relFilePath);

                    if (!CheckAllowAndDenyList(serverFilePath)) continue;

                    AddFileToAll(new SyncPairRunFile()
                    {
                        Name = localFile.Name,
                        RelativePath = relFilePath,
                    });

                    singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, false));
                }

                if (!withSubfolders) return;

                IAsyncOperation<IReadOnlyList<StorageFolder>> localSubFoldersTask = localFolder?.GetFoldersAsync();

                FolderSortItem[] serverSubFolders = serverFolderContent.Folders ?? new FolderSortItem[0];
                List<StorageFolder> localSubFolders = localSubFoldersTask != null ?
                    (await localSubFoldersTask).ToList() : new List<StorageFolder>();

                foreach (FolderSortItem serverSubFolder in serverSubFolders)
                {
                    int index;
                    string relSubFolderPath = api.Config.JoinPaths(relPath, serverSubFolder.Name);
                    StorageFolder localSubFolder = null;

                    if (localSubFolders.TryIndexOf(f => string.Equals(f.Name, serverSubFolder.Name, StringComparison.OrdinalIgnoreCase), out index))
                    {
                        localSubFolder = localSubFolders[index];
                        localSubFolders.RemoveAt(index);
                    }

                    await Query(relSubFolderPath, localSubFolder);
                }

                foreach (StorageFolder localSubFolder in localSubFolders)
                {
                    string relSubFolderPath = Path.Combine(relPath, localSubFolder.Name);

                    await Query(relSubFolderPath, localSubFolder);
                }
            }
        }

        /// <summary>
        /// Check if file has to be synced based on whitelist and blacklist
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if file has to be synced</returns>
        private bool CheckAllowAndDenyList(string path)
        {
            if (denialList.Any(e => path.EndsWith(e))) return false;

            return allowList.Length == 0 || allowList.Any(e => path.EndsWith(e));
        }

        private async Task CompareBothFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = bothFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await modeHandler.GetActionOfBothFiles(pair);
                    HandleAction(action, pair);
                    AddToList(pair, SyncPairRunFileType.Compared, false);
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Compare both file error", e);
                }
            }
        }

        private async Task CompareSingleFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = singleFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await modeHandler.GetActionOfSingleFiles(pair);
                    HandleAction(action, pair);
                    AddToList(pair, SyncPairRunFileType.Compared, false);
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Compare single file error", e);
                }
            }
        }

        private void HandleAction(SyncActionType action, FilePair pair)
        {
            switch (action)
            {
                case SyncActionType.CopyToLocal:
                    copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToLocalByConflict:
                    AddToList(pair, SyncPairRunFileType.Conflict, false);
                    copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServer:
                    copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServerByConflict:
                    AddToList(pair, SyncPairRunFileType.Conflict, false);
                    copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromLocal:
                    deleteLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromServer:
                    deleteSeverFiles.Enqueue(pair);
                    break;

                case SyncActionType.Equal:
                    AddResultFile(pair);
                    AddToList(pair, SyncPairRunFileType.Equal, true);
                    break;

                case SyncActionType.Ignore:
                    AddResultFile(pair);

                    AddToList(pair, SyncPairRunFileType.Ignore, true);
                    break;
            }
        }

        private async Task CopyFilesToLocal()
        {
            while (true)
            {
                if (copyToLocalFiles.Count == 0) UpdateCurrentCopyToLocalRelPath(null);

                (bool isEnd, FilePair pair) = copyToLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                UpdateCurrentCopyToLocalRelPath(pair.RelativePath);

                string errorMessage = "Unkown";
                string fileName;
                StorageFolder localFolder;
                StorageFile tmpFile = null;

                try
                {
                    if (!isTestRun)
                    {
                        errorMessage = "Create local folder error";
                        (localFolder, fileName) = await TryCreateLocalFolder(pair.RelativePath, this.localFolder);


                        errorMessage = "Create local tmpFile error";
                        tmpFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);


                        errorMessage = "Copy file to local error";
                        await api.DownloadFile(pair.ServerFullPath, tmpFile);
                        object localCompareValue = await fileComparer.GetLocalCompareValue(tmpFile);

                        if (fileName != tmpFile.Name)
                        {
                            await tmpFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                        }

                        pair.LocalCompareValue = localCompareValue;
                        if (pair.ServerCompareValue == null) pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);
                    }

                    AddResultFile(pair);
                    AddToList(pair, SyncPairRunFileType.CopiedLocal, true);
                    continue;
                }
                catch (Exception e)
                {
                    ErrorFile(pair, errorMessage, e);
                }

                try
                {
                    if (tmpFile != null) await tmpFile.DeleteAsync();
                }
                catch { }
            }

            System.Diagnostics.Debug.WriteLine($"To Local ended!!!!!!!!!!!!!");
        }

        private async Task<(StorageFolder folder, string fileName)> TryCreateLocalFolder(string relPath, StorageFolder localBaseFolder)
        {
            string[] parts = relPath.Split(api.Config.DirectorySeparatorChar);
            StorageFolder preFolder = localBaseFolder;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                preFolder = await preFolder.CreateFolderAsync(parts[i], CreationCollisionOption.OpenIfExists);
            }

            return (preFolder, parts[parts.Length - 1]);
        }

        private async Task CopyFilesToServer()
        {
            while (true)
            {
                if (copyToServerFiles.Count == 0) UpdatCurrentCopyToServerRelPath(null);

                (bool isEnd, FilePair pair) = copyToServerFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                UpdatCurrentCopyToServerRelPath(pair.RelativePath);

                try
                {
                    if (!isTestRun)
                    {
                        if (!await TryCreateServerFolder(pair.ServerFullPath))
                        {
                            ErrorFile(pair, "Create server folder, to copy file to, failed");
                            continue;
                        }
                        if (!await api.UploadFile(pair.ServerFullPath, pair.LocalFile))
                        {
                            ErrorFile(pair, "Uploading file to server failed");
                            continue;
                        }

                        if (pair.LocalCompareValue == null) pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);
                        pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);
                    }

                    AddResultFile(pair);
                    AddToList(pair, SyncPairRunFileType.CopiedServer, true);
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Copy file to server error", e);
                }
            }

            System.Diagnostics.Debug.WriteLine($"To Server ended!!!!!!!!!!!!!");
        }

        private async Task<bool> TryCreateServerFolder(string serverFilePath)
        {
            if (await api.FolderExists(api.Config.GetParentPath(serverFilePath))) return true;

            string[] parts = serverFilePath.Split(api.Config.DirectorySeparatorChar);
            string currentFolderPath = string.Empty;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentFolderPath = api.Config.JoinPaths(currentFolderPath, parts[i]);

                if (await api.FolderExists(currentFolderPath)) continue;
                if (i == 0 || !await api.CreateFolder(currentFolderPath)) return false;
            }

            return true;
        }

        private async Task DeleteLocalFiles()
        {
            while (true)
            {
                if (deleteLocalFiles.Count == 0) UpdateCurrentDeleteFromLocalRelPath(null);

                (bool isEnd, FilePair pair) = deleteLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                UpdateCurrentDeleteFromLocalRelPath(pair.RelativePath);

                try
                {
                    if (!isTestRun) await pair.LocalFile.DeleteAsync();
                    AddToList(pair, SyncPairRunFileType.DeletedLocal, true);
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Delete local file error", e);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Del local ended!!!!!!!!!!!!!");
        }

        private async Task DeleteServerFiles()
        {
            while (true)
            {
                if (deleteSeverFiles.Count == 0) UpdateCurrentDeleteFromServerRelPath(null);

                (bool isEnd, FilePair pair) = deleteSeverFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                UpdateCurrentDeleteFromServerRelPath(pair.RelativePath);

                if (isTestRun || await api.DeleteFile(pair.ServerFullPath))
                {
                    AddToList(pair, SyncPairRunFileType.DeletedServer, true);
                    continue;
                }

                ErrorFile(pair, "Delete file from server error");
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        private async Task UpdateDatabaseProgress()
        {
            while (true)
            {
                (bool isEnd, Func<Task> step) = dbDatabaseProgressSteps.Dequeue();
                if (isEnd) break;

                try
                {
                    await step();
                    Progress?.Invoke(this, EventArgs.Empty);
                }
                catch { }
            }

            System.Diagnostics.Debug.WriteLine($"Update database progress ended!!!!!!!!!!!!!");
        }

        public async Task Run()
        {
            Task updateDatabaseProgressTask = Task.Run(UpdateDatabaseProgress);

            try
            {
                if (requestedCancel)
                {
                    SetState(SyncPairHandlerState.Canceled);
                    return;
                }

                await database.SyncPairs.ResetSyncPairRun(SyncPairRunId);
                await database.SyncPairs.UpdateSyncPairRunLocalFolderPath(SyncPairRunId, localFolder.Path);

                SetState(SyncPairHandlerState.Running);

                await Task.WhenAll(Task.Run(QueryFiles), CompareFiles(), ProcessFiles());

                if (state == SyncPairHandlerState.Running)
                {
                    if (!isTestRun) await SaveNewResult();
                    SetState(SyncPairHandlerState.Finished);
                }
            }
            catch
            {
                SetState(SyncPairHandlerState.Error);
            }
            finally
            {
                dbDatabaseProgressSteps.End();
                await updateDatabaseProgressTask;
            }

            async Task CompareFiles()
            {
                await Task.WhenAll(Task.Run(CompareBothFiles), Task.Run(CompareSingleFiles));

                copyToLocalFiles.End();
                copyToServerFiles.End();

                deleteLocalFiles.End();
                deleteSeverFiles.End();
            }

            Task ProcessFiles()
            {
                return Task.WhenAll
                (
                    Task.Run(CopyFilesToLocal),
                    Task.Run(CopyFilesToServer),
                    Task.Run(DeleteLocalFiles),
                    Task.Run(DeleteServerFiles)
                );
            }
        }

        public void Cancel()
        {
            if (IsEnded) return;

            SetState(SyncPairHandlerState.Canceled);

            bothFiles.End();
            singleFiles.End();
            copyToLocalFiles.End();
            copyToServerFiles.End();
            deleteLocalFiles.End();
            deleteSeverFiles.End();
        }
    }
}

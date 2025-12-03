using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using FileSystemCommonUWP.Sync.Handling.Mode;
using FileSystemCommonUWP.Sync.Handling.Progress;
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
        private readonly BaseSyncFileComparer fileComparer;
        private readonly SyncConflictHandlingType conlictHandlingType;
        private readonly string[] allowList, denialList;
        private readonly Api api;
        private readonly SyncPairResult lastResult, newResult;
        private readonly HashSet<string> serverFolderExistsCache;

        private SyncPairHandlerState state;
        private readonly LockQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;

        public int SyncPairRunId { get; }

        public string RunToken { get; }

        public bool IsCanceled => state == SyncPairHandlerState.Canceled;

        public bool IsEnded => state == SyncPairHandlerState.Finished ||
            state == SyncPairHandlerState.Error ||
            state == SyncPairHandlerState.Canceled;

        public SyncPairProgressHandler ProgressHandler { get; }

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
            newResult = new SyncPairResult();
            ProgressHandler = new SyncPairProgressHandler(syncPairRunId, database);
            serverFolderExistsCache = new HashSet<string>();

            this.database = database;
            this.SyncPairRunId = syncPairRunId;
            this.withSubfolders = withSubfolders;
            this.requestedCancel = requestedCancel;
            this.isTestRun = isTestRun;
            fileComparer = GetFileComparer(compareType, api);
            conlictHandlingType = conflictHandlingType;
            modeHandler = GetSyncModeHandler(mode, fileComparer, lastResult, conflictHandlingType);
            this.localFolder = localFolder;
            this.serverPath = serverPath;
            this.allowList = allowList?.ToArray() ?? new string[0];
            this.denialList = denialList?.ToArray() ?? new string[0];
            this.api = api;
            this.lastResult = lastResult;
        }

        private static SyncModeHandler GetSyncModeHandler(SyncMode mode, BaseSyncFileComparer fileComparer,
            SyncPairResult lastResult, SyncConflictHandlingType conflictHandlingType)
        {
            switch (mode)
            {
                case SyncMode.ServerToLocalCreateOnly:
                    return new ServerToLocalCreateOnlyModeHandler(fileComparer, conflictHandlingType);

                case SyncMode.ServerToLocal:
                    return new ServerToLocalModeHandler(fileComparer, conflictHandlingType);

                case SyncMode.LocalToServerCreateOnly:
                    return new LocalToServerCreateOnlyModeHandler(fileComparer, conflictHandlingType);

                case SyncMode.LocalToServer:
                    return new LocalToServerModeHandler(fileComparer, conflictHandlingType);

                case SyncMode.TwoWay:
                    return new TwoWayModeHandler(fileComparer, lastResult, conflictHandlingType);
            }

            throw new ArgumentException("Value not Implemented: " + mode, nameof(mode));
        }

        private static BaseSyncFileComparer GetFileComparer(SyncCompareType type, Api api)
        {
            switch (type)
            {
                case SyncCompareType.Exists:
                    return new ExistsComparer(api);

                case SyncCompareType.Size:
                    return new SizeComparer(api);

                case SyncCompareType.Hash:
                    return new HashComparer(api);

                case SyncCompareType.PartialHash:
                    return new HashComparer(api, partialHashSize);
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
            ProgressHandler.SetState(state);
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
                ProgressHandler.UpdateCurrentQueryFolderRelPath(null);

                bothFiles.End();
                singleFiles.End();
                System.Diagnostics.Debug.WriteLine($"Query ended!!!!!!!");
            }

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                ProgressHandler.UpdateCurrentQueryFolderRelPath(relPath);

                string serverFolderPath = api.Config.JoinPaths(serverPath, relPath);

                Task<FolderContent> serverFolderContentTask = api.FolderContent(serverFolderPath);
                IAsyncOperation<IReadOnlyList<StorageFile>> localFilesTask = localFolder?.GetFilesAsync();

                FolderContent serverFolderContent = await serverFolderContentTask ?? new FolderContent();
                FileSortItem[] serverFiles = serverFolderContent.Files ?? new FileSortItem[0];
                IDictionary<string, StorageFile> localFiles = localFilesTask != null
                    ? (await localFilesTask).ToDictionary(f => f.Name)
                    : new Dictionary<string, StorageFile>();
                List<FilePair> newBothFiles = new List<FilePair>();
                List<FilePair> newSingleFiles = new List<FilePair>();

                if (IsCanceled) return;

                foreach (FileSortItem serverFile in serverFiles)
                {
                    if (!CheckAllowAndDenyList(serverFile.Path)) continue;

                    string relFilePath = api.Config.JoinPaths(relPath, serverFile.Name);
                    StorageFile localFile;

                    ProgressHandler.AddFileToAll(new SyncPairRunFile()
                    {
                        Name = serverFile.Name,
                        RelativePath = relFilePath,
                    });

                    if (localFiles.TryGetValue(serverFile.Name, out localFile))
                    {
                        localFiles.Remove(serverFile.Name);
                        newBothFiles.Add(CreateFilePair(serverPath, relFilePath, localFile, true));
                    }
                    else newSingleFiles.Add(CreateFilePair(serverPath, relFilePath, null, true));
                }

                foreach (StorageFile localFile in localFiles.Values)
                {
                    string relFilePath = api.Config.JoinPaths(relPath, localFile.Name);
                    string serverFilePath = api.Config.JoinPaths(serverPath, relFilePath);

                    if (!CheckAllowAndDenyList(serverFilePath)) continue;

                    ProgressHandler.AddFileToAll(new SyncPairRunFile()
                    {
                        Name = localFile.Name,
                        RelativePath = relFilePath,
                    });

                    newSingleFiles.Add(CreateFilePair(serverPath, relFilePath, localFile, false));
                }

                if (newBothFiles.Count > 0) bothFiles.Enqueue(newBothFiles);
                if (newSingleFiles.Count > 0) singleFiles.Enqueue(newSingleFiles);

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

        private async Task CompareFilesBatch(ICollection<FilePair> pairs, Func<FilePair, Task<SyncActionType>> getActionFunc)
        {
            if (pairs.Count == 0) return;

            Dictionary<string, FilePair> dict = pairs.ToDictionary(p => p.ServerFullPath);

            await fileComparer.GetServerCompareValues(dict.Keys.ToArray(), async (path, value, errorMessage) =>
            {
                if (!dict.TryGetValue(path, out FilePair pair)) return;

                if (value == null)
                {
                    ProgressHandler.AddFileToErrorList(pair, "Compare files batch no value: " + errorMessage);
                    return;
                }

                try
                {
                    pair.ServerCompareValue = value;
                    SyncActionType action = await getActionFunc(pair);
                    HandleAction(action, pair);
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Compared, false);
                }
                catch (Exception e)
                {
                    ProgressHandler.AddFileToErrorList(pair, "Compare files batch error", e);
                }
                finally
                {
                    dict.Remove(path);
                }
            });

            foreach (FilePair pair in dict.Values)
            {
                ProgressHandler.AddFileToErrorList(pair, "Compare files batch no response");
            }
        }

        private async Task CompareBothFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair[] batch) = bothFiles.DequeueBatch();
                if (IsCanceled) break;

                await CompareFilesBatch(batch, modeHandler.GetActionOfBothFiles);

                if (isEnd) break;
            }
        }

        private async Task CompareSingleFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair[] batch) = singleFiles.DequeueBatch();
                if (IsCanceled) break;

                IEnumerable<FilePair> singleComparePairs;
                if (modeHandler.PreloadServerCompareValue)
                {
                    FilePair[] batchComparePairs = batch.Where(f => f.ServerFileExists).ToArray();
                    singleComparePairs = batch.Where(f => !f.ServerFileExists);

                    await CompareFilesBatch(batchComparePairs, modeHandler.GetActionOfSingleFiles);
                }
                else singleComparePairs = batch;

                foreach (FilePair pair in singleComparePairs)
                {
                    try
                    {
                        SyncActionType action = await modeHandler.GetActionOfSingleFiles(pair);
                        HandleAction(action, pair);
                        ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Compared, false);
                    }
                    catch (Exception e)
                    {
                        ProgressHandler.AddFileToErrorList(pair, "Compare single file error", e);
                    }
                }

                if (isEnd) break;
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
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Conflict, false);
                    copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServer:
                    copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServerByConflict:
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Conflict, false);
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
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Equal, true);
                    break;

                case SyncActionType.Ignore:
                    AddResultFile(pair);

                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.Ignore, true);
                    break;
            }
        }

        private async Task CopyFilesToLocal()
        {
            while (true)
            {
                ProgressHandler.UpdateCurrentCopyToLocalRelPath(null);

                (bool isEnd, FilePair pair) = copyToLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                ProgressHandler.UpdateCurrentCopyToLocalRelPath(pair.RelativePath);

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
                        if (pair.ServerCompareValue == null) pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath);
                    }

                    AddResultFile(pair);
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.CopiedLocal, true);
                    continue;
                }
                catch (Exception e)
                {
                    ProgressHandler.AddFileToErrorList(pair, errorMessage, e);
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
                ProgressHandler.UpdateCurrentCopyToServerRelPath(null);

                (bool isEnd, FilePair pair) = copyToServerFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                ProgressHandler.UpdateCurrentCopyToServerRelPath(pair.RelativePath);

                try
                {
                    if (!isTestRun)
                    {
                        if (!await TryCreateServerFolder(pair.ServerFullPath))
                        {
                            ProgressHandler.AddFileToErrorList(pair, "Create server folder, to copy file to, failed");
                            continue;
                        }
                        if (!await api.UploadFile(pair.ServerFullPath, pair.LocalFile))
                        {
                            ProgressHandler.AddFileToErrorList(pair, "Uploading file to server failed");
                            continue;
                        }

                        if (pair.LocalCompareValue == null) pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);
                        pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath);
                    }

                    AddResultFile(pair);
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.CopiedServer, true);
                }
                catch (Exception e)
                {
                    ProgressHandler.AddFileToErrorList(pair, "Copy file to server error", e);
                }
            }

            System.Diagnostics.Debug.WriteLine($"To Server ended!!!!!!!!!!!!!");
        }

        private async Task<bool> TryCreateServerFolder(string serverFilePath)
        {
            string parentPath = api.Config.GetParentPath(serverFilePath);
            if (serverFolderExistsCache.Contains(parentPath)) return true;
            if (await api.FolderExists(parentPath))
            {
                serverFolderExistsCache.Add(parentPath);
                return true;
            }

            string[] parts = serverFilePath.Split(api.Config.DirectorySeparatorChar);
            string currentFolderPath = string.Empty;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentFolderPath = api.Config.JoinPaths(currentFolderPath, parts[i]);

                if (serverFolderExistsCache.Contains(currentFolderPath)) continue;
                if (await api.FolderExists(currentFolderPath))
                {
                    serverFolderExistsCache.Add(currentFolderPath);
                    continue;
                }
                if (i == 0 || !await api.CreateFolder(currentFolderPath)) return false;

                serverFolderExistsCache.Add(currentFolderPath);
            }

            return true;
        }

        private async Task DeleteLocalFiles()
        {
            while (true)
            {
                ProgressHandler.UpdateCurrentDeleteFromLocalRelPath(null);

                (bool isEnd, FilePair pair) = deleteLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                ProgressHandler.UpdateCurrentDeleteFromLocalRelPath(pair.RelativePath);

                try
                {
                    if (!isTestRun) await pair.LocalFile.DeleteAsync();
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.DeletedLocal, true);
                }
                catch (Exception e)
                {
                    ProgressHandler.AddFileToErrorList(pair, "Delete local file error", e);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Del local ended!!!!!!!!!!!!!");
        }

        private async Task DeleteServerFiles()
        {
            while (true)
            {
                ProgressHandler.UpdateCurrentDeleteFromServerRelPath(null);

                (bool isEnd, FilePair pair) = deleteSeverFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                ProgressHandler.UpdateCurrentDeleteFromServerRelPath(pair.RelativePath);

                if (isTestRun || await api.DeleteFile(pair.ServerFullPath))
                {
                    ProgressHandler.AddFileToList(pair, SyncPairRunFileType.DeletedServer, true);
                    continue;
                }

                ProgressHandler.AddFileToErrorList(pair, "Delete file from server error");
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        public async Task Run()
        {
            ProgressHandler.Start();

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
                await ProgressHandler.End();
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

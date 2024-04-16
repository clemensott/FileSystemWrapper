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
        private readonly int syncPairRunId;

        private readonly bool withSubfolders, isTestRun;
        private readonly string serverPath;
        private readonly StorageFolder localFolder;
        private readonly SyncModeHandler modeHandler;
        private readonly ISyncFileComparer fileComparer;
        private readonly SyncConflictHandlingType conlictHandlingType;
        private readonly string[] allowList, denialList;
        private readonly Api api;
        private readonly SyncPairResult lastResult, newResult;

        private SyncPairHandlerState state;
        private string currentQueryFolderRelPath;
        private readonly LockQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;
        private FilePair currentCopyToLocalFile, currentCopyToServerFile,
            currentDeleteFromServerFile, currentDeleteFromLocalFile;

        public string RunToken { get; }

        public SyncPairHandlerState State
        {
            get => state;
            private set
            {
                if (value == state) return;

                state = value;
            }
        }

        public bool IsCanceled => State == SyncPairHandlerState.Canceled;

        public bool IsEnded => State == SyncPairHandlerState.Finished ||
            State == SyncPairHandlerState.Error ||
            State == SyncPairHandlerState.Canceled;

        public string CurrentQueryFolderRelPath
        {
            get => currentQueryFolderRelPath;
            private set
            {
                if (value == currentQueryFolderRelPath) return;

                currentQueryFolderRelPath = value;
            }
        }

        public FilePair CurrentCopyToLocalFile
        {
            get => currentCopyToLocalFile;
            private set
            {
                if (value == currentCopyToLocalFile) return;

                currentCopyToLocalFile = value;
            }
        }

        public FilePair CurrentCopyToServerFile
        {
            get => currentCopyToServerFile;
            private set
            {
                if (value == currentCopyToServerFile) return;

                currentCopyToServerFile = value;
            }
        }

        public FilePair CurrentDeleteFromServerFile
        {
            get => currentDeleteFromServerFile;
            private set
            {
                if (value == currentDeleteFromServerFile) return;

                currentDeleteFromServerFile = value;
            }
        }

        public FilePair CurrentDeleteFromLocalFile
        {
            get => currentDeleteFromLocalFile;
            private set
            {
                if (value == currentDeleteFromLocalFile) return;

                currentDeleteFromLocalFile = value;
            }
        }

        public SyncPairHandler(AppDatabase database, int syncPairRunId, bool withSubfolders, bool isTestRun, bool requestedCancel,
            SyncPairResult lastResult, SyncMode mode, SyncCompareType compareType, SyncConflictHandlingType conflictHandlingType,
            StorageFolder localFolder, string serverPath, string[] allowList, string[] denialList, Api api)
        {
            State = requestedCancel ? SyncPairHandlerState.Canceled : SyncPairHandlerState.WaitForStart;
            bothFiles = new LockQueue<FilePair>();
            singleFiles = new LockQueue<FilePair>();
            copyToLocalFiles = new LockQueue<FilePair>();
            copyToServerFiles = new LockQueue<FilePair>();
            deleteLocalFiles = new LockQueue<FilePair>();
            deleteSeverFiles = new LockQueue<FilePair>();

            this.database = database;
            this.syncPairRunId = syncPairRunId;
            this.withSubfolders = withSubfolders;
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

        private async Task AddFileToAll(SyncPairRunFile file)
        {
            await database.SyncPairs.InsertSyncPairRunFile(syncPairRunId, file);
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
                CurrentQueryFolderRelPath = null;

                bothFiles.End();
                singleFiles.End();
                System.Diagnostics.Debug.WriteLine($"Query ended!!!!!!!");
            }

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                CurrentQueryFolderRelPath = relPath;

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

                    if (localFiles.TryGetValue(serverFile.Name, out localFile))
                    {
                        localFiles.Remove(serverFile.Name);
                        bothFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, true));
                    }
                    else singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, null, true));

                    await AddFileToAll(new SyncPairRunFile()
                    {
                        Name = serverFile.Name,
                        RelativePath = relFilePath,
                    });
                }

                foreach (StorageFile localFile in localFiles.Values)
                {
                    string relFilePath = api.Config.JoinPaths(relPath, localFile.Name);
                    string serverFilePath = api.Config.JoinPaths(serverPath, relFilePath);

                    if (!CheckAllowAndDenyList(serverFilePath)) continue;

                    singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, false));

                    await AddFileToAll(new SyncPairRunFile()
                    {
                        Name = localFile.Name,
                        RelativePath = relFilePath,
                    });
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
                    await HandleAction(action, pair);
                    await AddToList(pair, SyncPairRunFileType.Compared);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Compare both file error", e);
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
                    await HandleAction(action, pair);
                    await AddToList(pair, SyncPairRunFileType.Compared);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Compare single file error", e);
                }
            }
        }

        private async Task HandleAction(SyncActionType action, FilePair pair)
        {
            switch (action)
            {
                case SyncActionType.CopyToLocal:
                    copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToLocalByConflict:
                    await AddToList(pair, SyncPairRunFileType.Conflict);
                    copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServer:
                    copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServerByConflict:
                    await AddToList(pair, SyncPairRunFileType.Conflict);
                    copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromLocal:
                    deleteLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromServer:
                    deleteSeverFiles.Enqueue(pair);
                    break;

                case SyncActionType.Equal:
                    await AddToList(pair, SyncPairRunFileType.Equal);
                    break;

                case SyncActionType.Ignore:
                    await AddToList(pair, SyncPairRunFileType.Ignore);
                    break;
            }
        }

        private async Task CopyFilesToLocal()
        {
            while (true)
            {
                CurrentCopyToLocalFile = null;

                (bool isEnd, FilePair pair) = copyToLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    await AddToList(pair, SyncPairRunFileType.CopiedLocal);
                    continue;
                }

                CurrentCopyToLocalFile = pair;

                StorageFolder localFolder;
                string fileName;

                try
                {
                    (localFolder, fileName) = await TryCreateLocalFolder(pair.RelativePath, this.localFolder);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Create local folder error", e);
                    continue;
                }

                StorageFile tmpFile;

                try
                {
                    tmpFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Create local tmpFile error", e);
                    continue;
                }

                try
                {
                    await api.DownloadFile(pair.ServerFullPath, tmpFile);
                    object localCompareValue = await fileComparer.GetLocalCompareValue(tmpFile);

                    if (fileName != tmpFile.Name)
                    {
                        System.Diagnostics.Debug.WriteLine("Download file1");
                        await tmpFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                    }

                    System.Diagnostics.Debug.WriteLine("Download file2: " + tmpFile.Path);
                    pair.LocalCompareValue = localCompareValue;
                    System.Diagnostics.Debug.WriteLine("Download file3");
                    if (pair.ServerCompareValue == null) pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);

                    await AddToList(pair, SyncPairRunFileType.CopiedLocal);
                    continue;
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Copy file to local error", e);
                }

                try
                {
                    await tmpFile.DeleteAsync();
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
                CurrentCopyToServerFile = null;

                (bool isEnd, FilePair pair) = copyToServerFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    await AddToList(pair, SyncPairRunFileType.CopiedServer);
                    continue;
                }

                CurrentCopyToServerFile = pair;

                if (!await TryCreateServerFolder(pair.ServerFullPath))
                {
                    await ErrorFile(pair, "Create server folder, to copy file to, failed");
                    continue;
                }

                try
                {
                    if (!await api.UploadFile(pair.ServerFullPath, pair.LocalFile))
                    {
                        await ErrorFile(pair, "Uploading file to server failed");
                        continue;
                    }

                    if (pair.LocalCompareValue == null) pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);
                    pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);

                    await AddToList(pair, SyncPairRunFileType.CopiedServer);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Copy file to server error", e);
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
                CurrentDeleteFromLocalFile = null;

                (bool isEnd, FilePair pair) = deleteLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    await AddToList(pair, SyncPairRunFileType.DeletedLocal);
                    continue;
                }

                CurrentDeleteFromLocalFile = pair;

                try
                {
                    await pair.LocalFile.DeleteAsync();
                    await AddToList(pair, SyncPairRunFileType.DeletedLocal);
                }
                catch (Exception e)
                {
                    await ErrorFile(pair, "Delete local file error", e);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Del local ended!!!!!!!!!!!!!");
        }

        private async Task DeleteServerFiles()
        {
            while (true)
            {
                CurrentDeleteFromServerFile = null;

                (bool isEnd, FilePair pair) = deleteSeverFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    await AddToList(pair, SyncPairRunFileType.DeletedServer);
                    continue;
                }

                CurrentDeleteFromServerFile = pair;

                if (await api.DeleteFile(pair.ServerFullPath))
                {
                    await AddToList(pair, SyncPairRunFileType.DeletedServer);
                    continue;
                }

                await ErrorFile(pair, "Delete file from server error");
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        private async Task AddToList(FilePair pair, SyncPairRunFileType type)
        {
            await database.SyncPairs.SetSyncPairRunFileType(syncPairRunId, pair.RelativePath, type);
        }

        private async Task ErrorFile(FilePair pair, string message, Exception e = null)
        {
            await database.SyncPairs.SetSyncPairRunErrorFileType(syncPairRunId, pair.RelativePath, e);
        }

        private async Task IncreaseCurrentCount()
        {
            await database.SyncPairs.IncreaseSyncPairRunCurrentCount(syncPairRunId);
        }

        private async Task SaveNewResult()
        {
            await database.SyncPairs.InsertSyncPairResult(syncPairRunId, newResult);
        }

        public async Task Run()
        {
            try
            {
                if (State != SyncPairHandlerState.WaitForStart) return;

                State = SyncPairHandlerState.Running;

                await Task.WhenAll(Task.Run(QueryFiles), CompareFiles(), ProcessFiles());

                if (State == SyncPairHandlerState.Running)
                {
                    if (!isTestRun) await SaveNewResult();
                    State = SyncPairHandlerState.Finished;
                }
            }
            catch
            {
                State = SyncPairHandlerState.Error;
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

            State = SyncPairHandlerState.Canceled;

            bothFiles.End();
            singleFiles.End();
            copyToLocalFiles.End();
            copyToServerFiles.End();
            deleteLocalFiles.End();
            deleteSeverFiles.End();
        }
    }
}

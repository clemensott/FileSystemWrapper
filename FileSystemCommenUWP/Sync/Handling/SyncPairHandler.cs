using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.Communication;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using FileSystemCommonUWP.Sync.Handling.Mode;
using FileSystemCommonUWP.Sync.Result;
using StdOttStandard.Linq;
using StdOttStandard.Linq.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class SyncPairHandler
    {
        private const int partialHashSize = 10 * 1024; // 10 kB

        private readonly bool withSubfolders, isTestRun;
        private readonly string serverPath;
        private readonly StorageFolder localFolder;
        private readonly SyncModeHandler modeHandler;
        private readonly ISyncFileComparer fileComparer;
        private readonly SyncConflictHandlingType conlictHandlingType;
        private readonly string[] allowList, denialList;
        private readonly Api api;
        private readonly SyncedItems syncedItems;

        private SyncPairHandlerState state;
        private int currentCount, totalCount;
        private string currentQueryFolderRelPath;
        private readonly AsyncQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;
        private FilePair currentCopyToLocalFile, currentCopyToServerFile,
            currentDeleteFromServerFile, currentDeleteFromLocalFile;

        public event EventHandler<SyncPairProgressUpdate> Progress;

        public string RunToken { get; }

        public SyncPairHandlerState State
        {
            get => state;
            private set
            {
                if (value == state) return;

                state = value;
                OnPropertyChanged(nameof(State), value);
            }
        }

        public bool IsCanceled => State == SyncPairHandlerState.Canceled;

        public bool IsEnded => State == SyncPairHandlerState.Finished ||
            State == SyncPairHandlerState.Error ||
            State == SyncPairHandlerState.Canceled;

        public int CurrentCount
        {
            get => currentCount;
            private set
            {
                if (value == currentCount) return;

                currentCount = value;
                OnPropertyChanged(nameof(CurrentCount), value);
            }
        }

        public int TotalCount
        {
            get => totalCount;
            private set
            {
                if (value == totalCount) return;

                totalCount = value;
                OnPropertyChanged(nameof(TotalCount), value);
            }
        }

        public IDictionary<string, FilePair> AllFiles { get; }

        public IList<FilePair> ComparedFiles { get; }

        public IList<FilePair> EqualFiles { get; }

        public IList<FilePair> ConflictFiles { get; }

        public IList<FilePair> CopiedLocalFiles { get; }

        public IList<FilePair> CopiedServerFiles { get; }

        public IList<FilePair> DeletedLocalFiles { get; }

        public IList<FilePair> DeletedServerFiles { get; }

        internal IList<ErrorFilePair> ErrorFiles { get; }

        public IList<FilePair> IgnoreFiles { get; }

        public string CurrentQueryFolderRelPath
        {
            get => currentQueryFolderRelPath;
            private set
            {
                if (value == currentQueryFolderRelPath) return;

                currentQueryFolderRelPath = value;
                OnPropertyChanged(nameof(CurrentQueryFolderRelPath), value);
            }
        }

        public FilePair CurrentCopyToLocalFile
        {
            get => currentCopyToLocalFile;
            private set
            {
                if (value == currentCopyToLocalFile) return;

                currentCopyToLocalFile = value;
                OnPropertyChanged(nameof(CurrentCopyToLocalFile), value);
            }
        }

        public FilePair CurrentCopyToServerFile
        {
            get => currentCopyToServerFile;
            private set
            {
                if (value == currentCopyToServerFile) return;

                currentCopyToServerFile = value;
                OnPropertyChanged(nameof(CurrentCopyToServerFile), value);
            }
        }

        public FilePair CurrentDeleteFromServerFile
        {
            get => currentDeleteFromServerFile;
            private set
            {
                if (value == currentDeleteFromServerFile) return;

                currentDeleteFromServerFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromServerFile), value);
            }
        }

        public FilePair CurrentDeleteFromLocalFile
        {
            get => currentDeleteFromLocalFile;
            private set
            {
                if (value == currentDeleteFromLocalFile) return;

                currentDeleteFromLocalFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromLocalFile), value);
            }
        }

        private SyncPairHandler(string runToken, bool isCanceled, bool withSubfolders, bool isTestRun, SyncedItems syncedItems,
            SyncMode mode, SyncCompareType compareType, SyncConflictHandlingType conflictHandlingType,
            StorageFolder localFolder, string serverPath, string[] allowList, string[] denialList, Api api)
        {
            State = isCanceled ? SyncPairHandlerState.Canceled : SyncPairHandlerState.WaitForStart;
            bothFiles = new AsyncQueue<FilePair>();
            singleFiles = new AsyncQueue<FilePair>();
            copyToLocalFiles = new AsyncQueue<FilePair>();
            copyToServerFiles = new AsyncQueue<FilePair>();
            deleteLocalFiles = new AsyncQueue<FilePair>();
            deleteSeverFiles = new AsyncQueue<FilePair>();

            AllFiles = new Dictionary<string, FilePair>();
            ComparedFiles = new List<FilePair>();
            EqualFiles = new List<FilePair>();
            ConflictFiles = new List<FilePair>();
            CopiedLocalFiles = new List<FilePair>();
            CopiedServerFiles = new List<FilePair>();
            DeletedLocalFiles = new List<FilePair>();
            DeletedServerFiles = new List<FilePair>();
            ErrorFiles = new List<ErrorFilePair>();
            IgnoreFiles = new List<FilePair>();

            CurrentCount = 0;
            TotalCount = 0;

            RunToken = runToken;
            this.withSubfolders = withSubfolders;
            this.isTestRun = isTestRun;
            this.syncedItems = syncedItems;
            fileComparer = GetFileComparer(compareType);
            conlictHandlingType = conflictHandlingType;
            modeHandler = GetSyncModeHandler(mode, fileComparer, syncedItems, conflictHandlingType, api);
            this.localFolder = localFolder;
            this.serverPath = serverPath;
            this.allowList = allowList?.ToArray() ?? new string[0];
            this.denialList = denialList?.ToArray() ?? new string[0];
            this.api = api;
        }

        public static async Task<SyncPairHandler> FromSyncPairRequest(SyncPairRequestInfo request)
        {
            StorageFolder localFolder = await GetLocalFolder(request.Token);
            SyncedItems syncedItems = await SyncedItems.Create(request.ResultToken);
            Api api = await GetAPI(request.ApiBaseUrl);

            return new SyncPairHandler(request.RunToken, request.IsCanceled, request.WithSubfolders, request.IsTestRun,
                syncedItems, request.Mode, request.CompareType, request.ConflictHandlingType,
                localFolder, request.ServerPath, request.AllowList, request.DenialList, api);
        }

        private static async Task<StorageFolder> GetLocalFolder(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
            {
                throw new Exception("Local folder not found for requested token");
            }

            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        private static async Task<Api> GetAPI(string baseUrl)
        {
            Api api = new Api()
            {
                BaseUrl = baseUrl,
            };
            return await api.LoadConfig() ? api : throw new Exception("Couldn't load config from API");
        }

        private static SyncModeHandler GetSyncModeHandler(SyncMode mode, ISyncFileComparer fileComparer,
            SyncedItems syncedItems, SyncConflictHandlingType conflictHandlingType, Api api)
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
                    return new TwoWayModeHandler(fileComparer, syncedItems, conflictHandlingType, api);
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

                await bothFiles.End();
                await singleFiles.End();
                System.Diagnostics.Debug.WriteLine($"Query ended!!!!!!!");
            }

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                CurrentQueryFolderRelPath = relPath;

                int addedFilesCount = 0;
                string serverFolderPath = api.Config.JoinPaths(serverPath, relPath);

                Task<FolderContent> serverFolderContentTask = api.FolderContent(serverFolderPath);
                IAsyncOperation<IReadOnlyList<StorageFile>> localFilesTask = localFolder?.GetFilesAsync();

                FolderContent serverFolderContent = await serverFolderContentTask ?? new FolderContent();
                FileSortItem[] serverFiles = serverFolderContent.Files ?? new FileSortItem[0];
                List<StorageFile> localFiles = localFilesTask != null ?
                    (await localFilesTask).ToList() : new List<StorageFile>();

                if (IsCanceled) return;

                foreach (FileSortItem serverFile in serverFiles)
                {
                    if (!CheckWhitelistAndBlacklist(serverFile.Path)) continue;

                    int index;
                    string relFilePath = api.Config.JoinPaths(relPath, serverFile.Name);
                    StorageFile localFile;

                    if (localFiles.TryIndexOf(f => string.Equals(f.Name, serverFile.Name, StringComparison.OrdinalIgnoreCase), out index))
                    {
                        localFile = localFiles[index];
                        localFiles.RemoveAt(index);

                        await bothFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, true));
                    }
                    else await singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, null, true));
                    addedFilesCount++;
                }

                foreach (StorageFile localFile in localFiles)
                {
                    string relFilePath = api.Config.JoinPaths(relPath, localFile.Name);
                    string serverFilePath = api.Config.JoinPaths(serverPath, relFilePath);

                    if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                    await singleFiles.Enqueue(CreateFilePair(serverPath, relFilePath, localFile, false));
                    addedFilesCount++;
                }

                TotalCount += addedFilesCount;

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
        private bool CheckWhitelistAndBlacklist(string path)
        {
            if (denialList.Any(e => path.EndsWith(e))) return false;

            return allowList.Length == 0 || allowList.Any(e => path.EndsWith(e));
        }

        private async Task CompareBothFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = await bothFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await modeHandler.GetActionOfBothFiles(pair);
                    await HandleAction(action, pair);
                    AddToList(ComparedFiles, pair, nameof(ComparedFiles));
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
                (bool isEnd, FilePair pair) = await singleFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await modeHandler.GetActionOfSingleFiles(pair);
                    await HandleAction(action, pair);
                    AddToList(ComparedFiles, pair, nameof(ComparedFiles));
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Compare single file error", e);
                }
            }
        }

        private async Task HandleAction(SyncActionType action, FilePair pair)
        {
            switch (action)
            {
                case SyncActionType.CopyToLocal:
                    await copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToLocalByConflict:
                    AddToList(ConflictFiles, pair, nameof(ConflictFiles));
                    await copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServer:
                    await copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServerByConflict:
                    AddToList(ConflictFiles, pair, nameof(ConflictFiles));
                    await copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromLocal:
                    await deleteLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromServer:
                    await deleteSeverFiles.Enqueue(pair);
                    break;

                case SyncActionType.Equal:
                    EqualedFile(pair);
                    break;

                case SyncActionType.Ignore:
                    IgnoreFile(pair);
                    break;
            }
        }

        private async Task CopyFilesToLocal()
        {
            while (true)
            {
                CurrentCopyToLocalFile = null;

                (bool isEnd, FilePair pair) = await copyToLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    CopiedLocalFile(pair);
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
                    ErrorFile(pair, "Create local folder error", e);
                    continue;
                }

                StorageFile tmpFile;

                try
                {
                    tmpFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Create local tmpFile error", e);
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

                    CopiedLocalFile(pair);
                    continue;
                }
                catch (Exception e)
                {
                    ErrorFile(pair, "Copy file to local error", e);
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

                (bool isEnd, FilePair pair) = await copyToServerFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    CopiedServerFile(pair);
                    continue;
                }

                CurrentCopyToServerFile = pair;

                if (!await TryCreateServerFolder(pair.ServerFullPath))
                {
                    ErrorFile(pair, "Create server folder, to copy file to, failed");
                    continue;
                }

                try
                {
                    if (!await api.UploadFile(pair.ServerFullPath, pair.LocalFile))
                    {
                        ErrorFile(pair, "Uploading file to server failed");
                        continue;
                    }

                    if (pair.LocalCompareValue == null) pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);
                    pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);

                    CopiedServerFile(pair);
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
                CurrentDeleteFromLocalFile = null;

                (bool isEnd, FilePair pair) = await deleteLocalFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    DeletedLocalFile(pair);
                    continue;
                }

                CurrentDeleteFromLocalFile = pair;

                try
                {
                    await pair.LocalFile.DeleteAsync();
                    DeletedLocalFile(pair);
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
                CurrentDeleteFromServerFile = null;

                (bool isEnd, FilePair pair) = await deleteSeverFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (isTestRun)
                {
                    DeletedServerFile(pair);
                    continue;
                }

                CurrentDeleteFromServerFile = pair;

                if (await api.DeleteFile(pair.ServerFullPath))
                {
                    DeletedServerFile(pair);
                    continue;
                }

                ErrorFile(pair, "Delete file from server error");
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        private void CopiedLocalFile(FilePair pair)
        {
            AddResult(pair);

            AddToList(CopiedLocalFiles, pair, nameof(CopiedLocalFiles));
            CurrentCount++;
        }

        private void CopiedServerFile(FilePair pair)
        {
            AddResult(pair);

            AddToList(CopiedServerFiles, pair, nameof(CopiedServerFiles));
            CurrentCount++;
        }

        private void EqualedFile(FilePair pair)
        {
            AddResult(pair);

            AddToList(EqualFiles, pair, nameof(EqualFiles));
            CurrentCount++;
        }

        private void IgnoreFile(FilePair pair)
        {
            SyncedItem last;
            if (syncedItems.TryGetItem(pair.RelativePath, out last))
            {
                if (pair.ServerCompareValue == null) pair.ServerCompareValue = last.ServerCompareValue;
                if (pair.LocalCompareValue == null) pair.LocalCompareValue = last.LocalCompareValue;
            }

            AddToList(IgnoreFiles, pair, nameof(IgnoreFiles));
            CurrentCount++;
        }

        private void AddResult(FilePair pair)
        {
            syncedItems.Add(new SyncedItem()
            {
                RelativePath = pair.RelativePath,
                ServerCompareValue = pair.ServerCompareValue,
                LocalCompareValue = pair.LocalCompareValue,
                IsFile = true,
            });
        }

        private void DeletedLocalFile(FilePair pair)
        {
            AddToList(DeletedLocalFiles, pair, nameof(DeletedLocalFiles));
            CurrentCount++;
        }

        private void DeletedServerFile(FilePair pair)
        {
            AddToList(DeletedServerFiles, pair, nameof(DeletedServerFiles));
            CurrentCount++;
        }

        private void ErrorFile(FilePair pair, string message, Exception e = null)
        {
            AddToList(ErrorFiles, new ErrorFilePair(pair, new Exception(message, e)), nameof(ErrorFiles));
            CurrentCount++;
        }

        public async Task Run()
        {
            try
            {
                if (State != SyncPairHandlerState.WaitForStart) return;

                State = SyncPairHandlerState.Running;

                await Task.WhenAll(QueryFiles(), CompareFiles(), ProcessFiles());

                if (State == SyncPairHandlerState.Running)
                {
                    if (!isTestRun) await syncedItems.SaveNewResult();
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

                await copyToLocalFiles.End();
                await copyToServerFiles.End();

                await deleteLocalFiles.End();
                await deleteSeverFiles.End();
            }

            Task ProcessFiles()
            {
                return Task.WhenAll
                (
                    CopyFilesToLocal(),
                    CopyFilesToServer(),
                    DeleteLocalFiles(),
                    DeleteServerFiles()
                );
            }
        }

        public Task Cancel()
        {
            if (IsEnded) return Task.CompletedTask;

            State = SyncPairHandlerState.Canceled;

            return Task.WhenAll(bothFiles.End(),
                singleFiles.End(),
                copyToLocalFiles.End(),
                copyToServerFiles.End(),
                deleteLocalFiles.End(),
                deleteSeverFiles.End());
        }

        private void AddToList(IList<FilePair> list, FilePair pair, string name)
        {
            bool isNewFile = false;
            lock (this)
            {
                list.Add(pair);
                if (!AllFiles.ContainsKey(pair.RelativePath))
                {
                    AllFiles.Add(pair.RelativePath, pair);
                    isNewFile = true;
                }
            }
            if (isNewFile) OnAddToList(nameof(AllFiles), pair);
            OnAddToList(name, pair.RelativePath);
        }

        private void AddToList(IList<ErrorFilePair> list, ErrorFilePair pair, string name)
        {
            lock (this)
            {
                list.Add(pair);
            }
            OnAddToList(name, pair);
        }

        private void OnPropertyChanged(string name, int value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Set,
                Number = value,
            });
        }

        private void OnPropertyChanged(string name, string value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Set,
                Text = value,
            });
        }

        private void OnPropertyChanged(string name, SyncPairHandlerState value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Set,
                State = value,
            });
        }

        private void OnPropertyChanged(string name, FilePair value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Set,
                File = FilePairInfo.FromFilePair(value),
            });
        }

        private void OnAddToList(string name, string value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Add,
                Text = value,
            });
        }

        private void OnAddToList(string name, FilePair value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Add,
                File = FilePairInfo.FromFilePair(value),
            });
        }

        private void OnAddToList(string name, ErrorFilePair value)
        {
            OnPropertyChanged(new SyncPairProgressUpdate()
            {
                Token = RunToken,
                Prop = name,
                Action = SyncPairProgressUpdateAction.Add,
                ErrorFile = ErrorFilePairInfo.FromFilePair(value),
            });
        }

        private void OnPropertyChanged(SyncPairProgressUpdate update)
        {
            try
            {
                Progress?.Invoke(this, update);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("SyncPairHandler.OnPropertyChanged error: " + e);
            }
        }
    }
}

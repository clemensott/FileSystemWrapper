using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemUWP.API;
using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
using FileSystemUWP.Sync.Handling.Mode;
using FileSystemUWP.Sync.Result;
using StdOttStandard.Linq;
using StdOttStandard.Linq.DataStructures;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace FileSystemUWP.Sync.Handling
{
    class SyncPairHandler : INotifyPropertyChanged
    {
        private const int partialHashSize = 10 * 1024; // 10 kB

        private SyncPairHandlerState state;
        private int currentCount, totalCount;
        private string currentQueryFolderRelPath;
        private ObservableCollection<FilePair> comparedFiles, equalFiles, conflictFiles, copiedLocalFiles,
            copiedServerFiles, deletedLocalFiles, deletedServerFiles, ignoreFiles;
        private ObservableCollection<ErrorFilePair> errorFiles;
        private readonly AsyncQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;
        private FilePair currentCopyToLocalFile, currentCopyToServerFile,
            currentDeleteFromServerFile, currentDeleteFromLocalFile;
        private readonly IDictionary<string, SyncedItem> lastResult;
        private readonly IList<SyncedItem> newResult;
        private readonly SemaphoreSlim runSem;

        public SyncPairHandlerState State
        {
            get => state;
            private set
            {
                if (value == state) return;

                state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public bool IsTestRun { get; }

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
                OnPropertyChanged(nameof(CurrentCount));
            }
        }

        public int TotalCount
        {
            get => totalCount;
            private set
            {
                if (value == totalCount) return;

                totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        public ObservableCollection<FilePair> ComparedFiles
        {
            get => comparedFiles;
            private set
            {
                if (value == comparedFiles) return;

                comparedFiles = value;
                OnPropertyChanged(nameof(ComparedFiles));
            }
        }

        public ObservableCollection<FilePair> EqualFiles
        {
            get => equalFiles;
            private set
            {
                if (value == equalFiles) return;

                equalFiles = value;
                OnPropertyChanged(nameof(EqualFiles));
            }
        }

        public ObservableCollection<FilePair> ConflictFiles
        {
            get => conflictFiles;
            set
            {
                if (value == conflictFiles) return;

                conflictFiles = value;
                OnPropertyChanged(nameof(ConflictFiles));
            }
        }

        public ObservableCollection<FilePair> CopiedLocalFiles
        {
            get => copiedLocalFiles;
            private set
            {
                if (value == copiedLocalFiles) return;

                copiedLocalFiles = value;
                OnPropertyChanged(nameof(CopiedLocalFiles));
            }
        }

        public ObservableCollection<FilePair> CopiedServerFiles
        {
            get => copiedServerFiles;
            private set
            {
                if (value == copiedServerFiles) return;

                copiedServerFiles = value;
                OnPropertyChanged(nameof(CopiedServerFiles));
            }
        }

        public ObservableCollection<FilePair> DeletedLocalFiles
        {
            get => deletedLocalFiles;
            set
            {
                if (value == deletedLocalFiles) return;

                deletedLocalFiles = value;
                OnPropertyChanged(nameof(DeletedLocalFiles));
            }
        }

        public ObservableCollection<FilePair> DeletedServerFiles
        {
            get => deletedServerFiles;
            set
            {
                if (value == deletedServerFiles) return;

                deletedServerFiles = value;
                OnPropertyChanged(nameof(DeletedServerFiles));
            }
        }

        public ObservableCollection<ErrorFilePair> ErrorFiles
        {
            get => errorFiles;
            private set
            {
                if (value == errorFiles) return;

                errorFiles = value;
                OnPropertyChanged(nameof(ErrorFiles));
            }
        }

        public ObservableCollection<FilePair> IgnoreFiles
        {
            get => ignoreFiles;
            private set
            {
                if (value == ignoreFiles) return;

                ignoreFiles = value;
                OnPropertyChanged(nameof(IgnoreFiles));
            }
        }

        public string CurrentQueryFolderRelPath
        {
            get => currentQueryFolderRelPath;
            private set
            {
                if (value == currentQueryFolderRelPath) return;

                currentQueryFolderRelPath = value;
                OnPropertyChanged(nameof(CurrentQueryFolderRelPath));
            }
        }

        public FilePair CurrentCopyToLocalFile
        {
            get => currentCopyToLocalFile;
            private set
            {
                if (value == currentCopyToLocalFile) return;

                currentCopyToLocalFile = value;
                OnPropertyChanged(nameof(CurrentCopyToLocalFile));
            }
        }

        public FilePair CurrentCopyToServerFile
        {
            get => currentCopyToServerFile;
            private set
            {
                if (value == currentCopyToServerFile) return;

                currentCopyToServerFile = value;
                OnPropertyChanged(nameof(CurrentCopyToServerFile));
            }
        }

        public FilePair CurrentDeleteFromServerFile
        {
            get => currentDeleteFromServerFile;
            private set
            {
                if (value == currentDeleteFromServerFile) return;

                currentDeleteFromServerFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromServerFile));
            }
        }

        public FilePair CurrentDeleteFromLocalFile
        {
            get => currentDeleteFromLocalFile;
            private set
            {
                if (value == currentDeleteFromLocalFile) return;

                currentDeleteFromLocalFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromLocalFile));
            }
        }

        public bool WithSubfolders { get; }

        public string Token { get; }

        public string Name { get; }

        public string ServerNamePath { get; }

        public string ServerPath { get; }

        public StorageFolder LocalFolder { get; }

        public SyncModeHandler ModeHandler { get; }

        public ISyncFileComparer FileComparer { get; }

        public SyncConflictHandlingType ConlictHandlingType { get; }

        public string[] Whitelist { get; }

        public string[] Blacklist { get; }

        public ICollection<SyncedItem> NewResult => newResult;

        public Api Api { get; }

        public Task Task { get; }

        public SyncPairHandler(string token, bool withSubfolders, string name, PathPart[] serverPath,
            StorageFolder localFolder, SyncMode mode, ISyncFileComparer fileComparer,
            SyncConflictHandlingType conflictHandlingType, IEnumerable<string> whitelist,
            IEnumerable<string> blacklist, IEnumerable<SyncedItem> lastResult, Api api, bool isTestRun = false)
        {
            bothFiles = new AsyncQueue<FilePair>();
            singleFiles = new AsyncQueue<FilePair>();
            copyToLocalFiles = new AsyncQueue<FilePair>();
            copyToServerFiles = new AsyncQueue<FilePair>();
            deleteLocalFiles = new AsyncQueue<FilePair>();
            deleteSeverFiles = new AsyncQueue<FilePair>();
            newResult = new List<SyncedItem>();

            comparedFiles = new ObservableCollection<FilePair>();
            equalFiles = new ObservableCollection<FilePair>();
            conflictFiles = new ObservableCollection<FilePair>();
            copiedLocalFiles = new ObservableCollection<FilePair>();
            copiedServerFiles = new ObservableCollection<FilePair>();
            deletedLocalFiles = new ObservableCollection<FilePair>();
            deletedServerFiles = new ObservableCollection<FilePair>();
            errorFiles = new ObservableCollection<ErrorFilePair>();
            ignoreFiles = new ObservableCollection<FilePair>();

            CurrentCount = 0;
            TotalCount = 0;

            Token = token;
            WithSubfolders = withSubfolders;
            Name = name;
            ServerNamePath = serverPath.GetNamePath(api.Config.DirectorySeparatorChar);
            ServerPath = serverPath?.LastOrDefault().Path;
            LocalFolder = localFolder;
            FileComparer = fileComparer;
            ConlictHandlingType = conflictHandlingType;
            Whitelist = whitelist?.ToArray() ?? new string[0];
            Blacklist = blacklist?.ToArray() ?? new string[0];
            this.lastResult = lastResult?.Where(r => !string.IsNullOrWhiteSpace(r.RelativePath))
                .ToDictionary(r => r.RelativePath) ?? new Dictionary<string, SyncedItem>();
            Api = api;
            ModeHandler = GetSyncModeHandler(mode, fileComparer,
                this.lastResult, conflictHandlingType, api);
            IsTestRun = isTestRun;

            runSem = new SemaphoreSlim(0);
            Task = runSem.WaitAsync();
        }

        public static SyncPairHandler FromSyncPair(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return new SyncPairHandler(sync.Token, sync.WithSubfolders, sync.Name, sync.ServerPath,
                sync.LocalFolder, mode ?? sync.Mode, GetFileComparer(sync.CompareType),
                sync.ConflictHandlingType, sync.Whitelist, sync.Blacklist, sync.Result, api, isTestRun);
        }

        private static SyncModeHandler GetSyncModeHandler(SyncMode mode, ISyncFileComparer fileComparer,
            IDictionary<string, SyncedItem> lastResult, SyncConflictHandlingType conflictHandlingType, Api api)
        {
            switch (mode)
            {
                case SyncMode.ServerToLocalCreateOnly:
                    return new ServerToLocalCreateOnlyModeHandler(fileComparer, lastResult, conflictHandlingType, api);

                case SyncMode.ServerToLocal:
                    return new ServerToLocalModeHandler(fileComparer, lastResult, conflictHandlingType, api);

                case SyncMode.LocalToServerCreateOnly:
                    return new LocalToServerCreateOnlyModeHandler(fileComparer, lastResult, conflictHandlingType, api);

                case SyncMode.LocalToServer:
                    return new LocalToServerModeHandler(fileComparer, lastResult, conflictHandlingType, api);

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
            string relativePath = relPath.Trim(Api.Config.DirectorySeparatorChar);
            string serverFullPath = Api.Config.JoinPaths(serverBasePath, relPath);
            string name = Path.GetFileName(serverFullPath);
            return new FilePair(name, relativePath, serverFullPath, serverFileExists, localFile);
        }

        private async Task QueryFiles()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ServerPath) && LocalFolder != null) await Query(string.Empty, LocalFolder);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("QueryFiles error: " + e);
                State = SyncPairHandlerState.Error;
            }
            finally
            {
                CurrentQueryFolderRelPath = null;
            }

            await bothFiles.End();
            await singleFiles.End();
            System.Diagnostics.Debug.WriteLine($"Query ended!!!!!!!");

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                CurrentQueryFolderRelPath = relPath;

                int addedFilesCount = 0;
                string serverFolderPath = Api.Config.JoinPaths(ServerPath, relPath);

                Task<FolderContent> serverFolderContentTask = Api.FolderContent(serverFolderPath);
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
                    string relFilePath = Api.Config.JoinPaths(relPath, serverFile.Name);
                    StorageFile localFile;

                    if (localFiles.TryIndexOf(f => string.Equals(f.Name, serverFile.Name, StringComparison.OrdinalIgnoreCase), out index))
                    {
                        localFile = localFiles[index];
                        localFiles.RemoveAt(index);

                        await bothFiles.Enqueue(CreateFilePair(ServerPath, relFilePath, localFile, true));
                    }
                    else await singleFiles.Enqueue(CreateFilePair(ServerPath, relFilePath, null, true));
                    addedFilesCount++;
                }

                foreach (StorageFile localFile in localFiles)
                {
                    string relFilePath = Api.Config.JoinPaths(relPath, localFile.Name);
                    string serverFilePath = Api.Config.JoinPaths(ServerPath, relFilePath);

                    if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                    await singleFiles.Enqueue(CreateFilePair(ServerPath, relFilePath, localFile, false));
                    addedFilesCount++;
                }

                TotalCount += addedFilesCount;

                if (!WithSubfolders) return;

                IAsyncOperation<IReadOnlyList<StorageFolder>> localSubFoldersTask = localFolder?.GetFoldersAsync();

                FolderSortItem[] serverSubFolders = serverFolderContent.Folders ?? new FolderSortItem[0];
                List<StorageFolder> localSubFolders = localSubFoldersTask != null ?
                    (await localSubFoldersTask).ToList() : new List<StorageFolder>();

                foreach (FolderSortItem serverSubFolder in serverSubFolders)
                {
                    int index;
                    string relSubFolderPath = Api.Config.JoinPaths(relPath, serverSubFolder.Name);
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
            if (Blacklist.Any(e => path.EndsWith(e))) return false;

            return Whitelist.Length == 0 || Whitelist.Any(e => path.EndsWith(e));
        }

        private async Task CompareBothFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = await bothFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await ModeHandler.GetActionOfBothFiles(pair);
                    await HandleAction(action, pair);
                    await AddSafe(ComparedFiles, pair);
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
                (bool isEnd, FilePair pair) = await singleFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                try
                {
                    SyncActionType action = await ModeHandler.GetActionOfSingleFiles(pair);
                    await HandleAction(action, pair);
                    await AddSafe(ComparedFiles, pair);
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
                    await copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToLocalByConflict:
                    await AddSafe(ConflictFiles, pair);
                    await copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServer:
                    await copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.CopyToServerByConflict:
                    await AddSafe(ConflictFiles, pair);
                    await copyToServerFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromLocal:
                    await deleteLocalFiles.Enqueue(pair);
                    break;

                case SyncActionType.DeleteFromServer:
                    await deleteSeverFiles.Enqueue(pair);
                    break;

                case SyncActionType.Equal:
                    await EqualedFile(pair);
                    break;

                case SyncActionType.Ignore:
                    await IgnoreFile(pair);
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

                if (IsTestRun)
                {
                    await CopiedLocalFile(pair);
                    continue;
                }

                CurrentCopyToLocalFile = pair;

                StorageFolder localFolder;
                string fileName;

                try
                {
                    (localFolder, fileName) = await TryCreateLocalFolder(pair.RelativePath, LocalFolder);
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
                    await Api.DownloadFile(pair.ServerFullPath, tmpFile);
                    object localCompareValue = await FileComparer.GetLocalCompareValue(tmpFile);

                    if (fileName != tmpFile.Name)
                    {
                        System.Diagnostics.Debug.WriteLine("Download file1");
                        await tmpFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                    }

                    System.Diagnostics.Debug.WriteLine("Download file2: " + tmpFile.Path);
                    pair.LocalCompareValue = localCompareValue;
                    System.Diagnostics.Debug.WriteLine("Download file3");
                    if (pair.ServerCompareValue == null) pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);

                    await CopiedLocalFile(pair);
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
            string[] parts = relPath.Split(Api.Config.DirectorySeparatorChar);
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

                if (IsTestRun)
                {
                    await CopiedServerFile(pair);
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
                    if (!await Api.UploadFile(pair.ServerFullPath, pair.LocalFile))
                    {
                        await ErrorFile(pair, "Uploading file to server failed");
                        continue;
                    }

                    if (pair.LocalCompareValue == null) pair.LocalCompareValue = await FileComparer.GetLocalCompareValue(pair.LocalFile);
                    pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);

                    await CopiedServerFile(pair);
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
            if (await Api.FolderExists(Api.Config.GetParentPath(serverFilePath))) return true;

            string[] parts = serverFilePath.Split(Api.Config.DirectorySeparatorChar);
            string currentFolderPath = string.Empty;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentFolderPath = Api.Config.JoinPaths(currentFolderPath, parts[i]);

                if (await Api.FolderExists(currentFolderPath)) continue;
                if (i == 0 || !await Api.CreateFolder(currentFolderPath)) return false;
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

                if (IsTestRun)
                {
                    await DeletedLocalFile(pair);
                    continue;
                }

                CurrentDeleteFromLocalFile = pair;

                try
                {
                    await pair.LocalFile.DeleteAsync();
                    await DeletedLocalFile(pair);
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

                (bool isEnd, FilePair pair) = await deleteSeverFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                if (IsTestRun)
                {
                    await DeletedServerFile(pair);
                    continue;
                }

                CurrentDeleteFromServerFile = pair;

                if (await Api.DeleteFile(pair.ServerFullPath))
                {
                    await DeletedServerFile(pair);
                    continue;
                }

                await ErrorFile(pair, "Delete file from server error");
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        private async Task CopiedLocalFile(FilePair pair)
        {
            AddResult(pair);

            await AddSafe(CopiedLocalFiles, pair);
            CurrentCount++;
        }

        private async Task CopiedServerFile(FilePair pair)
        {
            AddResult(pair);

            await AddSafe(CopiedServerFiles, pair);
            CurrentCount++;
        }

        private async Task EqualedFile(FilePair pair)
        {
            AddResult(pair);

            await AddSafe(EqualFiles, pair);
            CurrentCount++;
        }

        private async Task IgnoreFile(FilePair pair)
        {
            SyncedItem last;
            if (lastResult.TryGetValue(pair.RelativePath, out last))
            {
                if (pair.ServerCompareValue == null) pair.ServerCompareValue = last.ServerCompareValue;
                if (pair.LocalCompareValue == null) pair.LocalCompareValue = last.LocalCompareValue;
            }

            await AddSafe(IgnoreFiles, pair);
            CurrentCount++;
        }

        private void AddResult(FilePair pair)
        {
            newResult.Add(new SyncedItem()
            {
                RelativePath = pair.RelativePath,
                ServerCompareValue = pair.ServerCompareValue,
                LocalCompareValue = pair.LocalCompareValue,
                IsFile = true,
            });
        }

        private async Task DeletedLocalFile(FilePair pair)
        {
            await AddSafe(DeletedLocalFiles, pair);
            CurrentCount++;
        }

        private async Task DeletedServerFile(FilePair pair)
        {
            await AddSafe(DeletedServerFiles, pair);
            CurrentCount++;
        }

        private async Task ErrorFile(FilePair pair, string message, Exception e = null)
        {
            await AddSafe(ErrorFiles, new ErrorFilePair(pair, new Exception(message, e)));
            CurrentCount++;
        }

        public async Task Start()
        {
            if (State != SyncPairHandlerState.WaitForStart) return;

            State = SyncPairHandlerState.Running;

            await Task.WhenAll
            (
                QueryFiles(),
                CompareFiles(),
                ProcessFiles()
            );

            if (State == SyncPairHandlerState.Running) State = SyncPairHandlerState.Finished;

            runSem.Release();

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

        private static Task AddSafe<T>(IList<T> list, T pair)
        {
            return UwpUtils.RunSafe(() => list.Add(pair));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void OnPropertyChanged(string name)
        {
            await UwpUtils.RunSafe(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }
    }
}

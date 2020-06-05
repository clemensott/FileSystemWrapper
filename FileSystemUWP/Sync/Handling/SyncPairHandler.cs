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
using Windows.Storage.Streams;

namespace FileSystemUWP.Sync.Handling
{
    class SyncPairHandler : INotifyPropertyChanged
    {
        private SyncPairHandlerState state;
        private int currentCount, totalCount;
        private string currentQueryFolderRelPath;
        private ObservableCollection<FilePair> comparedFiles, equalFiles, conflictFiles,
            copiedFiles, deletedFiles, errorFiles, ignoreFiles;
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

        public ObservableCollection<FilePair> CopiedFiles
        {
            get => copiedFiles;
            private set
            {
                if (value == copiedFiles) return;

                copiedFiles = value;
                OnPropertyChanged(nameof(CopiedFiles));
            }
        }

        public ObservableCollection<FilePair> DeletedFiles
        {
            get => deletedFiles;
            set
            {
                if (value == deletedFiles) return;

                deletedFiles = value;
                OnPropertyChanged(nameof(DeletedFiles));
            }
        }

        public ObservableCollection<FilePair> ErrorFiles
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

        public SyncPairHandler(string token, bool withSubfolders, string name, string serverPath,
            StorageFolder localFolder, SyncMode mode, ISyncFileComparer fileComparer,
            SyncConflictHandlingType conflictHandlingType, IEnumerable<string> whitelist,
            IEnumerable<string> blacklist, IEnumerable<SyncedItem> lastResult, Api api)
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
            copiedFiles = new ObservableCollection<FilePair>();
            deletedFiles = new ObservableCollection<FilePair>();
            errorFiles = new ObservableCollection<FilePair>();
            ignoreFiles = new ObservableCollection<FilePair>();

            CurrentCount = 0;
            TotalCount = 0;

            Token = token;
            WithSubfolders = withSubfolders;
            Name = name;
            ServerPath = serverPath;
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

            runSem = new SemaphoreSlim(0);
            Task = runSem.WaitAsync();
        }

        public static SyncPairHandler FromSyncPair(SyncPair sync, Api api)
        {
            return new SyncPairHandler(sync.Token, sync.WithSubfolders, sync.Name, sync.ServerPath,
                sync.LocalFolder, sync.Mode, GetFileComparer(sync.CompareType),
                sync.ConflictHandlingType, sync.Whitelist, sync.Blacklist, sync.Result, api);
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
            }

            throw new ArgumentException("Value not Implemented:" + type, nameof(type));
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
            System.Diagnostics.Debug.WriteLine($"Query endded!!!!!!!");

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                CurrentQueryFolderRelPath = relPath;

                int addedFilesCount = 0;
                string serverFolderPath = Path.Combine(ServerPath, relPath);
                
                Task<List<string>> serverFilesTask = Api.ListFiles(serverFolderPath);
                IAsyncOperation<IReadOnlyList<StorageFile>> localFilesTask = localFolder?.GetFilesAsync();
                
                List<string> serverFiles = await serverFilesTask ?? new List<string>();
                List<StorageFile> localFiles = localFilesTask != null ?
                    (await localFilesTask).ToList() : new List<StorageFile>();
                
                if (IsCanceled) return;

                foreach (string serverFilePath in serverFiles)
                {
                    if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                    int index;
                    string name = Path.GetFileName(serverFilePath);
                    string relFilePath = Path.Combine(relPath, name);
                    StorageFile localFile;

                    if (localFiles.TryIndexOf(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase), out index))
                    {
                        localFile = localFiles[index];
                        localFiles.RemoveAt(index);

                        await bothFiles.Enqueue(new FilePair(ServerPath, relFilePath, localFile, true));
                    }
                    else await singleFiles.Enqueue(new FilePair(ServerPath, relFilePath, null, true));
                    addedFilesCount++;
                }
                
                foreach (StorageFile localFile in localFiles)
                {
                    string relFilePath = Path.Combine(relPath, localFile.Name);
                    string serverFilePath = Path.Combine(ServerPath, relFilePath);

                    if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                    await singleFiles.Enqueue(new FilePair(ServerPath, relFilePath, localFile, false));
                    addedFilesCount++;
                }
                
                TotalCount += addedFilesCount;

                if (!WithSubfolders) return;
                
                Task<List<string>> serverSubFolderTask = Api.ListFolders(serverFolderPath);
                IAsyncOperation<IReadOnlyList<StorageFolder>> localSubFoldersTask = localFolder?.GetFoldersAsync();
                
                List<string> serverSubFolders = await serverSubFolderTask ?? new List<string>();
                List<StorageFolder> localSubFolders = localSubFoldersTask != null ?
                    (await localSubFoldersTask).ToList() : new List<StorageFolder>();
                
                foreach (string serverSubFolderPath in serverSubFolders)
                {
                    int index;
                    string name = Path.GetFileName(serverSubFolderPath);
                    string relSubFolderPath = Path.Combine(relPath, name);
                    StorageFolder localSubFolder = null;

                    if (localSubFolders.TryIndexOf(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase), out index))
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
                catch
                {
                    await ErrorFile(pair);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Compare both endded: {singleFiles.IsEnd} | {singleFiles.Count}");
            if (singleFiles.IsEnd && singleFiles.Count == 0)
            {
                await copyToLocalFiles.End();
                await copyToServerFiles.End();

                await deleteLocalFiles.End();
                await deleteSeverFiles.End();
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
                catch
                {
                    await ErrorFile(pair);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Compare single endded: {bothFiles.IsEnd} | {bothFiles.Count}");
            if (bothFiles.IsEnd && bothFiles.Count == 0)
            {
                await copyToLocalFiles.End();
                await copyToServerFiles.End();

                await deleteLocalFiles.End();
                await deleteSeverFiles.End();
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

                CurrentCopyToLocalFile = pair;

                (StorageFolder localFolder, string fileName) = await TryCreateLocalFolder(pair.RelativePath, LocalFolder);

                if (localFolder == null)
                {
                    await ErrorFile(pair);
                    continue;
                }

                StorageFile tmpFile;

                try
                {
                    tmpFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                }
                catch
                {
                    await ErrorFile(pair);
                    continue;
                }

                try
                {
                    await Api.DownlaodFile(pair.ServerFullPath, tmpFile);

                    if (fileName != tmpFile.Name)
                    {
                        await tmpFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                    }

                    pair.LocalCompareValue = await FileComparer.GetLocalCompareValue(tmpFile);
                    if (pair.ServerCompareValue == null) pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);

                    await CopiedFile(pair);
                    continue;
                }
                catch { }

                try
                {
                    await tmpFile.DeleteAsync();
                }
                catch { }

                await ErrorFile(pair);
            }

            System.Diagnostics.Debug.WriteLine($"To Local ended!!!!!!!!!!!!!");
        }

        private static async Task<(StorageFolder folder, string fileName)> TryCreateLocalFolder(string relPath, StorageFolder localBaseFolder)
        {
            string[] parts = relPath.Split('\\');
            StorageFolder preFolder = localBaseFolder;

            try
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    preFolder = await preFolder.CreateFolderAsync(parts[i], CreationCollisionOption.OpenIfExists);
                }

                return (preFolder, parts[parts.Length - 1]);
            }
            catch
            {
                return (null, null);
            }
        }

        private async Task CopyFilesToServer()
        {
            while (true)
            {
                CurrentCopyToServerFile = null;

                (bool isEnd, FilePair pair) = await copyToServerFiles.Dequeue();
                if (isEnd || IsCanceled) break;

                CurrentCopyToServerFile = pair;

                if (!await TryCreateServerFolder(pair.ServerFullPath))
                {
                    await ErrorFile(pair);
                    continue;
                }

                try
                {
                    IInputStream readStream = await pair.LocalFile.OpenReadAsync();

                    if (await Api.WriteFile(pair.ServerFullPath, readStream))
                    {
                        if (pair.LocalCompareValue == null) pair.LocalCompareValue = await FileComparer.GetLocalCompareValue(pair.LocalFile);
                        pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);

                        await CopiedFile(pair);
                        continue;
                    }
                }
                catch { }

                await ErrorFile(pair);
            }

            System.Diagnostics.Debug.WriteLine($"To Server ended!!!!!!!!!!!!!");
        }

        private async Task<bool> TryCreateServerFolder(string serverFilePath)
        {
            string[] parts = serverFilePath.Split('\\');
            string currentFolderPath = string.Empty;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentFolderPath = Path.Combine(currentFolderPath, parts[i]);

                if (await Api.FolderExists(currentFolderPath)) continue;
                if (!await Api.CreateFolder(currentFolderPath)) return false;
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

                CurrentDeleteFromLocalFile = pair;

                try
                {
                    await pair.LocalFile.DeleteAsync();
                    await DeletedFile(pair);
                }
                catch
                {
                    await ErrorFile(pair);
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

                CurrentDeleteFromServerFile = pair;

                if (await Api.DeleteFile(pair.ServerFullPath))
                {
                    await DeletedFile(pair);
                    continue;
                }

                await ErrorFile(pair);
            }

            System.Diagnostics.Debug.WriteLine($"Del Server ended!!!!!!!!!!!!!");
        }

        private async Task CopiedFile(FilePair pair)
        {
            AddResult(pair);

            await AddSafe(CopiedFiles, pair);
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

        private async Task DeletedFile(FilePair pair)
        {
            await AddSafe(DeletedFiles, pair);
            CurrentCount++;
        }

        private async Task ErrorFile(FilePair pair)
        {
            await AddSafe(ErrorFiles, pair);
            CurrentCount++;
        }

        public async Task Start()
        {
            if (State != SyncPairHandlerState.WaitForStart) return;

            State = SyncPairHandlerState.Running;

            await Task.WhenAll(QueryFiles(),
                Task.Run(() => CompareBothFiles()),
                Task.Run(() => CompareSingleFiles()),
                CopyFilesToLocal(),
                CopyFilesToServer(),
                DeleteLocalFiles(),
                DeleteServerFiles());

            if (State == SyncPairHandlerState.Running) State = SyncPairHandlerState.Finished;

            runSem.Release();
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

        private static Task AddSafe(IList<FilePair> list, FilePair pair)
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

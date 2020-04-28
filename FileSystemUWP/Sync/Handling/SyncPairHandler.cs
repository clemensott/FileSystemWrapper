using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
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
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileSystemUWP.Sync.Handling
{
    public class SyncPairHandler : INotifyPropertyChanged
    {
        private SyncPairHandlerState state;
        private int currentCount, totalCount;
        private ObservableCollection<FilePair> comparedFiles, equalFiles, confictFiles,
            copiedFiles, deletedFiles, errorFiles;

        private readonly AsyncQueue<FilePair> bothFiles, singleFiles, copyToLocalFiles,
            copyToServerFiles, deleteLocalFiles, deleteSeverFiles;
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
            State == SyncPairHandlerState.Failed ||
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

        public ObservableCollection<FilePair> ConfictFiles
        {
            get => confictFiles;
            set
            {
                if (value == confictFiles) return;

                confictFiles = value;
                OnPropertyChanged(nameof(ConfictFiles));
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

        public bool WithSubfolders { get; }

        public string Token { get; }

        public string Name { get; }

        public string ServerPath { get; }

        public StorageFolder LocalFolder { get; }

        public SyncMode Mode { get; }

        public ISyncFileComparer FileComparer { get; }

        public SyncConfictHandlingType ConlictHandlingType { get; }

        public string[] Whitelist { get; }

        public string[] Blacklist { get; }

        public ICollection<SyncedItem> NewResult => newResult;

        public Api Api { get; }

        public Task Task { get; }

        public SyncPairHandler(string token, bool withSubfolders, string name, string serverPath,
            StorageFolder localFolder, SyncMode mode, ISyncFileComparer fileComparer,
            SyncConfictHandlingType confictHandlingType, IEnumerable<string> whitelist,
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
            confictFiles = new ObservableCollection<FilePair>();
            copiedFiles = new ObservableCollection<FilePair>();
            deletedFiles = new ObservableCollection<FilePair>();
            errorFiles = new ObservableCollection<FilePair>();

            CurrentCount = 0;
            TotalCount = 0;

            Token = token;
            WithSubfolders = withSubfolders;
            Name = name;
            ServerPath = serverPath;
            LocalFolder = localFolder;
            Mode = mode;
            FileComparer = fileComparer;
            ConlictHandlingType = confictHandlingType;
            Whitelist = whitelist?.ToArray() ?? new string[0];
            Blacklist = blacklist?.ToArray() ?? new string[0];
            this.lastResult = lastResult?.Where(r => !string.IsNullOrWhiteSpace(r.RelativePath))
                .ToDictionary(r => r.RelativePath) ?? new Dictionary<string, SyncedItem>();
            Api = api;

            runSem = new SemaphoreSlim(0);
            Task = runSem.WaitAsync();
        }

        public static SyncPairHandler FromSyncPair(SyncPair sync, Api api)
        {
            return new SyncPairHandler(sync.Token, sync.WithSubfolders, sync.Name, sync.ServerPath,
                sync.LocalFolder, sync.Mode, GetFileComparer(sync.CompareType), sync.ConflictHandlingType,
                sync.Whitelist, sync.Blacklist, sync.Result, api);
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

            throw new ArgumentException("Value not Implemented", nameof(type));
        }

        private async Task QueryFiles()
        {
            if (!string.IsNullOrWhiteSpace(ServerPath) && LocalFolder != null) await Query(string.Empty, LocalFolder);

            await bothFiles.End();
            await singleFiles.End();
            System.Diagnostics.Debug.WriteLine("Query endded!!!!!!!");

            async Task Query(string relPath, StorageFolder localFolder)
            {
                if (IsCanceled) return;

                int addedFilesCount = 0;
                string serverFolderPath = Path.Combine(ServerPath, relPath);
                List<string> serverFiles = await Api.ListFiles(serverFolderPath) ?? new List<string>();
                List<StorageFile> localFiles = localFolder != null ?
                    (await localFolder.GetFilesAsync()).ToList() : new List<StorageFile>();

                if (IsCanceled) return;

                if (Mode == SyncMode.ServerToLocal || Mode == SyncMode.TwoWay)
                {
                    foreach (string serverFilePath in serverFiles)
                    {
                        if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                        int index;
                        string name = Path.GetFileName(serverFilePath);
                        string relFilePath = Path.Combine(relPath, name);
                        StorageFile localFile;

                        if (localFiles.TryIndexOf(f => f.Name == name, out index))
                        {
                            localFile = localFiles[index];
                            localFiles.RemoveAt(index);

                            await bothFiles.Enqueue(new FilePair(ServerPath, relFilePath, localFile, true));
                        }
                        else await singleFiles.Enqueue(new FilePair(ServerPath, relFilePath, null, true));
                        addedFilesCount++;
                    }

                    serverFiles.Clear();
                }

                if (Mode == SyncMode.LocalToServer || Mode == SyncMode.TwoWay)
                {
                    foreach (StorageFile localFile in localFiles)
                    {
                        string relFilePath = Path.Combine(relPath, localFile.Name);
                        string serverFilePath = Path.Combine(ServerPath, relFilePath);

                        if (!CheckWhitelistAndBlacklist(serverFilePath)) continue;

                        if (serverFiles.Contains(serverFilePath))
                        {
                            await bothFiles.Enqueue(new FilePair(ServerPath, relFilePath, localFile, true));
                        }
                        else
                        {
                            await singleFiles.Enqueue(new FilePair(ServerPath, relFilePath, localFile, false));
                        }

                        addedFilesCount++;
                    }
                }

                TotalCount += addedFilesCount;

                if (!WithSubfolders) return;

                List<string> serverSubFolders = await Api.ListFolders(serverFolderPath);
                List<StorageFolder> localSubFolders = localFolder != null ?
                    (await localFolder.GetFoldersAsync()).ToList() : new List<StorageFolder>();

                foreach (string serverSubFolderPath in serverSubFolders)
                {
                    int index;
                    string name = Path.GetFileName(serverSubFolderPath);
                    string relSubFolderPath = Path.Combine(relPath, name);
                    StorageFolder localSubFolder = null;

                    if (localSubFolders.TryIndexOf(f => f.Name == name, out index))
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

                if (isEnd) break;

                try
                {
                    await DecideNextActionOfBothFiles(pair);
                    await AddSafe(ComparedFiles, pair);
                }
                catch
                {
                    await AddSafe(ErrorFiles, pair);
                    CurrentCount++;
                    continue;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Compare both endded: {singleFiles.IsEnd}");
            if (singleFiles.IsEnd && singleFiles.Count == 0)
            {
                await copyToLocalFiles.End();
                await copyToServerFiles.End();

                await deleteLocalFiles.End();
                await deleteSeverFiles.End();
            }
        }

        private async Task DecideNextActionOfBothFiles(FilePair pair)
        {
            SyncedItem last;

            Task<object> serverCompareValueTask = FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);
            Task<object> localCompareValueTask = FileComparer.GetLocalCompareValue(pair.LocalFile);

            pair.ServerCompareValue = await serverCompareValueTask;
            pair.LocalCompareValue = await localCompareValueTask;

            if (FileComparer.Equals(pair.ServerCompareValue, pair.LocalCompareValue))
            {
                await EqualedFile(pair);
            }
            else if (lastResult.TryGetValue(pair.RelativePath, out last))
            {
                if (FileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue) &&
                    FileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    await EqualedFile(pair);
                }
                else if (FileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue))
                {
                    await copyToLocalFiles.Enqueue(pair);
                }
                else if (FileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    await copyToServerFiles.Enqueue(pair);
                }
                else await SolveConflict(pair);
            }
            else await SolveConflict(pair);
        }

        private async Task CompareSingleFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = await singleFiles.Dequeue();

                if (isEnd) break;

                try
                {
                    await DecideNextActionOfSingleFile(pair);
                    await AddSafe(ComparedFiles, pair);
                }
                catch
                {
                    await AddSafe(ErrorFiles, pair);
                    CurrentCount++;
                    continue;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Compare single endded: {bothFiles.IsEnd}");
            if (bothFiles.IsEnd && bothFiles.Count == 0)
            {
                await copyToLocalFiles.End();
                await copyToServerFiles.End();

                await deleteLocalFiles.End();
                await deleteSeverFiles.End();
            }
        }

        private async Task DecideNextActionOfSingleFile(FilePair pair)
        {
            SyncedItem last;

            if (pair.ServerFileExists)
            {
                pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);

                if (lastResult.TryGetValue(pair.ServerFullPath, out last))
                {
                    if (FileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue)) await deleteSeverFiles.Enqueue(pair);
                    else await copyToLocalFiles.Enqueue(pair);
                }
                else await copyToLocalFiles.Enqueue(pair);
            }
            else if (pair.LocalFile != null)
            {
                pair.LocalCompareValue = await FileComparer.GetLocalCompareValue(pair.LocalFile);

                if (lastResult.TryGetValue(pair.ServerFullPath, out last))
                {
                    if (FileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue)) await deleteLocalFiles.Enqueue(pair);
                    else await copyToServerFiles.Enqueue(pair);
                }
                else await copyToServerFiles.Enqueue(pair);
            }
        }

        private async Task SolveConflict(FilePair pair)
        {
            await AddSafe(ConfictFiles, pair);

            switch (ConlictHandlingType)
            {
                case SyncConfictHandlingType.PreferServer:
                    await copyToLocalFiles.Enqueue(pair);
                    break;

                case SyncConfictHandlingType.PreferLocal:
                    await copyToServerFiles.Enqueue(pair);
                    break;
            }
        }

        private async Task CopyFilesToLocal()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = await copyToLocalFiles.Dequeue();
                if (isEnd) break;

                System.Diagnostics.Debug.WriteLine($"To Local: {pair.RelativePath}");
                (StorageFolder localFolder, string fileName) = await TryCreateLocalFolder(pair.RelativePath, LocalFolder);

                if (localFolder == null)
                {
                    await AddSafe(ComparedFiles, pair);
                    CurrentCount++;
                    continue;
                }

                StorageFile tmpFile;

                try
                {
                    tmpFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                }
                catch
                {
                    await AddSafe(ComparedFiles, pair);
                    CurrentCount++;
                    continue;
                }

                try
                {
                    IInputStream readStream = await Api.GetFileInputStream(pair.ServerFullPath);
                    Stream writeStream = await tmpFile.OpenStreamForWriteAsync();

                    await readStream.AsStreamForRead().CopyToAsync(writeStream);

                    if (fileName != tmpFile.Name)
                    {
                        await tmpFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                    }

                    pair.LocalCompareValue = await FileComparer.GetLocalCompareValue(tmpFile);
                    await CopiedFile(pair);
                    continue;
                }
                catch { }

                try
                {
                    await tmpFile.DeleteAsync();
                }
                catch { }

                await AddSafe(ComparedFiles, pair);
                CurrentCount++;
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
                (bool isEnd, FilePair pair) = await copyToServerFiles.Dequeue();
                if (isEnd) break;

                System.Diagnostics.Debug.WriteLine($"To Server: {pair.RelativePath}");
                if (!await TryCreateServerFolder(pair.ServerFullPath))
                {
                    await AddSafe(ComparedFiles, pair);
                    CurrentCount++;
                    continue;
                }

                try
                {
                    IInputStream readStream = await pair.LocalFile.OpenReadAsync();

                    if (await Api.WriteFile(pair.ServerFullPath, readStream))
                    {
                        pair.ServerCompareValue = await FileComparer.GetServerCompareValue(pair.ServerFullPath, Api);
                        await CopiedFile(pair);
                        continue;
                    }
                }
                catch { }

                await AddSafe(ComparedFiles, pair);
                CurrentCount++;
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
                (bool isEnd, FilePair pair) = await deleteLocalFiles.Dequeue();
                if (isEnd) break;

                try
                {
                    System.Diagnostics.Debug.WriteLine($"Del Local: {pair.RelativePath}");
                    await pair.LocalFile.DeleteAsync();
                    await DeletedFile(pair);
                    continue;
                }
                catch { }

                await AddSafe(ComparedFiles, pair);
                CurrentCount++;
            }

            System.Diagnostics.Debug.WriteLine($"Del local ended!!!!!!!!!!!!!");
        }

        private async Task DeleteServerFiles()
        {
            while (true)
            {
                (bool isEnd, FilePair pair) = await deleteSeverFiles.Dequeue();
                if (isEnd) break;

                System.Diagnostics.Debug.WriteLine($"Del Server: {pair.RelativePath}");
                if (await Api.DeleteFile(pair.ServerFullPath))
                {
                    await DeletedFile(pair);
                    continue;
                }

                await AddSafe(ComparedFiles, pair);
                CurrentCount++;
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

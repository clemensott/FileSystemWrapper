using FileSystemUWP.API;
using FileSystemUWP.Controls;
using FileSystemUWP.Picker;
using FileSystemUWP.Sync.Definitions;
using System.Collections.Generic;
using System.ComponentModel;

namespace FileSystemUWP
{
    class Server : INotifyPropertyChanged
    {
        private bool isLoading, isRestoreItemFile;
        private string currentFolderPath;
        private FileSystemItemName? restoreFileSystemItem;
        private Api api;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (value == isLoading) return;

                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsRestoreItemFile
        {
            get => isRestoreItemFile;
            set
            {
                if (value == isRestoreItemFile) return;

                isRestoreItemFile = value;
                OnPropertyChanged(nameof(IsRestoreItemFile));
            }
        }

        public string CurrentFolderPath
        {
            get => currentFolderPath;
            set
            {
                if (value == currentFolderPath) return;

                currentFolderPath = value;
                OnPropertyChanged(nameof(CurrentFolderPath));
            }
        }

        public FileSystemItemName? RestoreFileSystemItem
        {
            get => restoreFileSystemItem;
            set
            {
                if (Equals(value, restoreFileSystemItem)) return;

                restoreFileSystemItem = value;
                OnPropertyChanged(nameof(RestoreFileSystemItem));
            }
        }

        public Api Api
        {
            get => api;
            set
            {
                if (value == api) return;

                api = value;
                OnPropertyChanged(nameof(Api));
            }
        }

        public SyncPairs Syncs { get; }

        public BackgroundOperations BackgroundOperations { get; }

        public string[] LoadedSyncPairTokens { get; set; }

        public Server(BackgroundOperations backgroundOperations)
        {
            Syncs = new SyncPairs();
            BackgroundOperations = backgroundOperations;
        }

        public Server(BackgroundOperations backgroundOperations, IEnumerable<SyncPair> pairs)
        {
            Syncs = new SyncPairs(pairs);
            BackgroundOperations = backgroundOperations;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

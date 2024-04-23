using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Database.Servers;
using FileSystemUWP.Controls;
using FileSystemUWP.FileViewers;
using FileSystemUWP.Picker;
using System.ComponentModel;
using System.Linq;

namespace FileSystemUWP
{
    class Server : INotifyPropertyChanged
    {
        private bool isLoading, isRestoreItemFile;
        private string currentFolderPath;
        private FileSystemItemSortBy sortBy;
        private FileSystemSortItem? restoreFileSystemItem;
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

        public FileSystemItemSortBy SortBy
        {
            get => sortBy;
            set
            {
                if (Equals(value, sortBy)) return;

                sortBy = value;
                OnPropertyChanged(nameof(SortBy));
            }
        }

        public FileSystemSortItem? RestoreFileSystemItem
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

        public int Id { get; set; }

        internal FilesViewing LastFilesViewing { get; set; }

        public BackgroundOperations BackgroundOperations { get; set; }

        public Server()
        {
        }

        public Server(ServerInfo server)
        {
            Id = server.Id;
            Api = server.Api;
            SortBy = server.SortBy;
            CurrentFolderPath = server.CurrentFolderPath;

            if (server.RestoreIsFile.HasValue && server.RestoreName != null)
            {
                var sortKeys = server.RestoreSortKeys?.ToList().AsReadOnly();
                RestoreFileSystemItem = new FileSystemSortItem(server.RestoreIsFile.Value, server.RestoreName, sortKeys);
            }
        }

        public ServerInfo ToInfo()
        {
            return new ServerInfo()
            {
                Id = Id,
                Api = Api,
                SortBy = SortBy,
                CurrentFolderPath = CurrentFolderPath,
                RestoreIsFile = RestoreFileSystemItem?.IsFile,
                RestoreName = RestoreFileSystemItem?.Name,
                RestoreSortKeys = RestoreFileSystemItem?.SortKeys?.ToArray(),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

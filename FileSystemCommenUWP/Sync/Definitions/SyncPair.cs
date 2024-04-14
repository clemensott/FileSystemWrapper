using FileSystemCommon.Models.FileSystem;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FileSystemCommonUWP.Sync.Definitions
{
    public class SyncPair : INotifyPropertyChanged
    {
        private bool withSubfolders, isLocalFolderLoaded;
        private string name;
        private PathPart[] serverPath;
        private StorageFolder localFolder;
        private SyncMode mode;
        private SyncCompareType compareType;
        private SyncConflictHandlingType conflictHandlingType;
        private ObservableCollection<string> allowList, denyList;

        public bool WithSubfolders
        {
            get => withSubfolders;
            set
            {
                if (value == withSubfolders) return;

                withSubfolders = value;
                OnPropertyChanged(nameof(WithSubfolders));
            }
        }

        [XmlIgnore]
        public bool IsLocalFolderLoaded
        {
            get => isLocalFolderLoaded;
            private set
            {
                if (value == isLocalFolderLoaded) return;

                isLocalFolderLoaded = value;
                OnPropertyChanged(nameof(IsLocalFolderLoaded));
            }
        }

        public int Id { get; set; }

        public string Token { get; }

        public string ResultToken { get; }

        public string Name
        {
            get => name;
            set
            {
                if (value == name) return;

                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public PathPart[] ServerPath
        {
            get => serverPath;
            set
            {
                if (value == serverPath) return;

                serverPath = value;
                OnPropertyChanged(nameof(ServerPath));
            }
        }

        [XmlIgnore]
        public StorageFolder LocalFolder
        {
            get => localFolder;
            set
            {
                if (value == localFolder) return;

                localFolder = value;
                OnPropertyChanged(nameof(LocalFolder));
            }
        }

        public SyncMode Mode
        {
            get => mode;
            set
            {
                if (value == mode) return;

                mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        public SyncCompareType CompareType
        {
            get => compareType;
            set
            {
                if (value == compareType) return;

                compareType = value;
                OnPropertyChanged(nameof(CompareType));
            }
        }

        public SyncConflictHandlingType ConflictHandlingType
        {
            get => conflictHandlingType;
            set
            {
                if (value == conflictHandlingType) return;

                conflictHandlingType = value;
                OnPropertyChanged(nameof(ConflictHandlingType));
            }
        }

        public ObservableCollection<string> AllowList
        {
            get => allowList;
            set
            {
                if (value == allowList) return;

                allowList = value;
                OnPropertyChanged(nameof(AllowList));
            }
        }

        public ObservableCollection<string> DenyList
        {
            get => denyList;
            set
            {
                if (value == denyList) return;

                denyList = value;
                OnPropertyChanged(nameof(DenyList));
            }
        }

        public SyncPair() : this(null, null)
        {
        }

        public SyncPair(string resultToken) : this(null, resultToken)
        {
        }

        public SyncPair(string token, string resultToken)
        {
            Token = token ?? Guid.NewGuid().ToString();
            ResultToken = resultToken ?? Guid.NewGuid().ToString();
        }

        public async Task LoadLocalFolder()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(Token))
            {
                LocalFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
            }
            IsLocalFolderLoaded = true;
        }

        public void SaveLocalFolder()
        {
            if (LocalFolder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(Token, LocalFolder);
                IsLocalFolderLoaded = true;
            }
            else if (IsLocalFolderLoaded)
            {
                try
                {
                    if (StorageApplicationPermissions.FutureAccessList.ContainsItem(Token))
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove(Token);
                    }
                }
                catch { }
            }

        }

        public SyncPair Clone()
        {
            return new SyncPair(ResultToken)
            {
                WithSubfolders = WithSubfolders,
                Name = Name,
                ServerPath = ServerPath,
                LocalFolder = LocalFolder,
                Mode = Mode,
                CompareType = CompareType,
                ConflictHandlingType = ConflictHandlingType,
                AllowList = AllowList != null ? new ObservableCollection<string>(AllowList) : null,
                DenyList = DenyList != null ? new ObservableCollection<string>(DenyList) : null,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

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
        private string name, localFolderPath;
        private PathPart[] serverPath;
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

        public int Id { get; set; }

        public int ServerId { get; }

        public int? CurrentSyncPairRunId { get; set; }

        public int? LastSyncPairResultId { get; set; }

        public string LocalFolderToken { get; }

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

        public string LocalFolderPath
        {
            get => localFolderPath;
            set
            {
                if (value == localFolderPath) return;

                localFolderPath = value;
                OnPropertyChanged(nameof(LocalFolderPath));
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

        public SyncPair(int serverId) : this(serverId, Guid.NewGuid().ToString())
        {
        }

        public SyncPair(int serverId, string localFolderToken)
        {
            ServerId = serverId;
            LocalFolderToken = localFolderToken;
        }

        public SyncPair Clone()
        {
            return new SyncPair(ServerId, LocalFolderToken)
            {
                Id = Id,
                CurrentSyncPairRunId = CurrentSyncPairRunId,
                LastSyncPairResultId = LastSyncPairResultId,
                WithSubfolders = WithSubfolders,
                Name = Name,
                ServerPath = ServerPath,
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

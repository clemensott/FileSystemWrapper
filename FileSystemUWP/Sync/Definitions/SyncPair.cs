using FileSystemUWP.Sync.Result;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Windows.Storage;

namespace FileSystemUWP.Sync.Definitions
{
    public class SyncPair : INotifyPropertyChanged
    {
        private bool withSubfolders;
        private string token, name, serverPath;
        private StorageFolder localFolder;
        private SyncMode mode;
        private SyncCompareType compareType;
        private SyncConfictHandlingType conflictHandlingType;
        private ObservableCollection<string> whitelist, blacklist;
        private SyncedItem[] result;

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

        public string Token
        {
            get => token;
            set
            {
                if (value == token) return;

                token = value;
                OnPropertyChanged(nameof(Token));
            }
        }

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

        public string ServerPath
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

        public SyncConfictHandlingType ConflictHandlingType
        {
            get => conflictHandlingType;
            set
            {
                if (value == conflictHandlingType) return;

                conflictHandlingType = value;
                OnPropertyChanged(nameof(ConflictHandlingType));
            }
        }

        public ObservableCollection<string> Whitelist
        {
            get => whitelist;
            set
            {
                if (value == whitelist) return;

                whitelist = value;
                OnPropertyChanged(nameof(Whitelist));
            }
        }

        public ObservableCollection<string> Blacklist
        {
            get => blacklist;
            set
            {
                if (value == blacklist) return;

                blacklist = value;
                OnPropertyChanged(nameof(Blacklist));
            }
        }

        public SyncedItem[] Result
        {
            get => result;
            set
            {
                if (value == result) return;

                result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public SyncPair()
        {
            Token = Guid.NewGuid().ToString();
            Result = new SyncedItem[0];
        }

        public SyncPair Clone()
        {
            return new SyncPair()
            {
                WithSubfolders = WithSubfolders,
                Name = Name,
                ServerPath = ServerPath,
                LocalFolder = LocalFolder,
                Mode = Mode,
                CompareType = CompareType,
                ConflictHandlingType = ConflictHandlingType,
                Whitelist = Whitelist != null ? new ObservableCollection<string>(Whitelist) : null,
                Blacklist = Blacklist != null ? new ObservableCollection<string>(Blacklist) : null,
                Result = Result?.ToArray(),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

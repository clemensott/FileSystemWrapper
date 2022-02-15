﻿using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.Sync.Result;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FileSystemUWP.Sync.Definitions
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

        public string Token { get; }

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

        public SyncPair() : this(Guid.NewGuid().ToString())
        {
        }

        public SyncPair(string token)
        {
            Token = token;
            Result = new SyncedItem[0];
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

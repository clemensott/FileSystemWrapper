using FileSystemUWP.Controls;
using FileSystemUWP.Sync.Definitions;
using System.Collections.Generic;
using System.ComponentModel;

namespace FileSystemUWP
{
    class ViewModel : INotifyPropertyChanged
    {
        private string currentFolderPath;
        private SyncPairs syncs;

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

        public SyncPairs Syncs
        {
            get => syncs;
            private set
            {
                if (value == syncs) return;

                syncs = value;
                OnPropertyChanged(nameof(Syncs));
            }
        }

        public Api Api { get; }

        public BackgroundOperations BackgroundOperations { get; }

        public ViewModel(IEnumerable<SyncPair> syncPairs)
        {
            Api = new Api();
            Syncs = new SyncPairs();
            BackgroundOperations = new BackgroundOperations();

            foreach (SyncPair sync in syncPairs)
            {
                Syncs.Add(sync);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.Communication;
using System.ComponentModel;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairPageSyncViewModel : INotifyPropertyChanged
    {
        private SyncPair syncPair;
        private SyncPairForegroundContainer run;

        public SyncPair SyncPair
        {
            get => syncPair;
            set
            {
                if (value == syncPair) return;

                syncPair = value;
                OnPropertyChanged(nameof(SyncPair));
            }
        }

        public SyncPairForegroundContainer Run
        {
            get => run;
            set
            {
                if (value == run) return;

                run = value;
                OnPropertyChanged(nameof(Run));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

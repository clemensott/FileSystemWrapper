using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairsPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SyncPairPageSyncViewModel> syncs;

        public ObservableCollection<SyncPairPageSyncViewModel> Syncs
        {
            get => syncs;
            set
            {
                if (value == syncs) return;

                syncs = value;
                OnPropertyChanged(nameof(Syncs));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

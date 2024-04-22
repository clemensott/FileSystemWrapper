using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairsPageViewModel : INotifyPropertyChanged
    {
        private bool isLoading;
        private ObservableCollection<SyncPairPageSyncViewModel> syncs;

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

        public SyncPairsPageViewModel()
        {
            IsLoading = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

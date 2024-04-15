using FileSystemCommonUWP.Sync.Definitions;
using System.ComponentModel;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class SyncPairRun : INotifyPropertyChanged
    {
        private SyncPairHandlerState state;
        private int currentCount, allFilesCount, comparedFilesCount, equalFilesCount,
            conflictFilesCount, copiedLocalFilesCount, copiedServerFilesCount,
            deletedLocalFilesCount, deletedServerFilesCount, errorFilesCount, ignoreFilesCount;

        public int Id { get; set; }

        #region Request
        public bool WithSubfolders { get; set; }

        public bool IsTestRun { get; set; }

        public bool RequestedCancel { get; set; }

        public SyncMode Mode { get; set; }

        public SyncCompareType CompareType { get; set; }

        public SyncConflictHandlingType ConflictHandlingType { get; set; }

        public string Name { get; set; }

        public string LocalFolderToken { get; set; }

        public string ServerNamePath { get; set; }

        public string ServerPath { get; set; }

        public string[] AllowList { get; set; }

        public string[] DenyList { get; set; }

        public string ApiBaseUrl { get; set; }
        #endregion

        #region Response
        public SyncPairHandlerState State
        {
            get => state;
            set
            {
                if (value == state) return;

                state = value;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(IsEnded));
            }
        }

        public int CurrentCount
        {
            get => currentCount;
            set
            {
                if (value == currentCount) return;

                currentCount = value;
                OnPropertyChanged(nameof(CurrentCount));
            }
        }

        public int AllFilesCount
        {
            get => allFilesCount;
            set
            {
                if (value == allFilesCount) return;

                allFilesCount = value;
                OnPropertyChanged(nameof(AllFilesCount));
            }
        }

        public int ComparedFilesCount
        {
            get => comparedFilesCount;
            set
            {
                if (value == comparedFilesCount) return;

                comparedFilesCount = value;
                OnPropertyChanged(nameof(ComparedFilesCount));
            }
        }

        public int EqualFilesCount
        {
            get => equalFilesCount;
            set
            {
                if (value == equalFilesCount) return;

                equalFilesCount = value;
                OnPropertyChanged(nameof(EqualFilesCount));
            }
        }

        public int ConflictFilesCount
        {
            get => conflictFilesCount;
            set
            {
                if (value == conflictFilesCount) return;

                conflictFilesCount = value;
                OnPropertyChanged(nameof(ConflictFilesCount));
            }
        }

        public int CopiedLocalFilesCount
        {
            get => copiedLocalFilesCount;
            set
            {
                if (value == copiedLocalFilesCount) return;

                copiedLocalFilesCount = value;
                OnPropertyChanged(nameof(CopiedLocalFilesCount));
            }
        }

        public int CopiedServerFilesCount
        {
            get => copiedServerFilesCount;
            set
            {
                if (value == copiedServerFilesCount) return;

                copiedServerFilesCount = value;
                OnPropertyChanged(nameof(CopiedServerFilesCount));
            }
        }

        public int DeletedLocalFilesCount
        {
            get => deletedLocalFilesCount;
            set
            {
                if (value == deletedLocalFilesCount) return;

                deletedLocalFilesCount = value;
                OnPropertyChanged(nameof(DeletedLocalFilesCount));
            }
        }

        public int DeletedServerFilesCount
        {
            get => deletedServerFilesCount;
            set
            {
                if (value == deletedServerFilesCount) return;

                deletedServerFilesCount = value;
                OnPropertyChanged(nameof(DeletedServerFilesCount));
            }
        }

        public int ErrorFilesCount
        {
            get => errorFilesCount;
            set
            {
                if (value == errorFilesCount) return;

                errorFilesCount = value;
                OnPropertyChanged(nameof(ErrorFilesCount));
            }
        }

        public int IgnoreFilesCount
        {
            get => ignoreFilesCount;
            set
            {
                if (value == ignoreFilesCount) return;

                ignoreFilesCount = value;
                OnPropertyChanged(nameof(IgnoreFilesCount));
            }
        }
        #endregion

        public bool IsEnded => State == SyncPairHandlerState.Finished ||
            State == SyncPairHandlerState.Error ||
            State == SyncPairHandlerState.Canceled;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

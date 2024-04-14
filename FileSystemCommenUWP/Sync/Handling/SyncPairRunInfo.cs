using FileSystemCommonUWP.Sync.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling
{
    class SyncPairRunInfo : INotifyPropertyChanged
    {
        private SyncPairHandlerState state;
        private int currentCount, totalCount;

        public int Id { get; set; }

        public int SyncPairId { get; set; }

        #region Request
        public bool WithSubfolders { get; set; }

        public bool IsTestRun { get; set; }

        public bool IsCanceled { get; set; }

        public SyncMode Mode { get; set; }

        public SyncCompareType CompareType { get; set; }

        public SyncConflictHandlingType ConflictHandlingType { get; set; }

        public string Name { get; set; }

        public string LocalPath { get; set; }

        public string ServerNamePath { get; set; }

        public string ServerPath { get; set; }

        public string[] AllowList { get; set; }

        public string[] DenialList { get; set; }

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

        public int TotalCount
        {
            get => totalCount;
            set
            {
                if (value == totalCount) return;

                totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

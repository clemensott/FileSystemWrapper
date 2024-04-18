using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using StdOttStandard.Linq;
using System.ComponentModel;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairPageSyncViewModel : INotifyPropertyChanged
    {
        private SyncPair syncPair;
        private SyncPairRun run;

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

        public SyncPairRun Run
        {
            get => run;
            set
            {
                if (value == run) return;

                run = value;
                OnPropertyChanged(nameof(Run));
            }
        }

        public void UpdateSyncPair(SyncPair syncPair)
        {
            if (SyncPair == null || syncPair == null)
            {
                SyncPair = syncPair;
                return;
            }

            SyncPair.Id = syncPair.Id;
            SyncPair.WithSubfolders = syncPair.WithSubfolders;
            SyncPair.CurrentSyncPairRunId = syncPair.CurrentSyncPairRunId;
            SyncPair.LastSyncPairResultId = syncPair.LastSyncPairResultId;
            SyncPair.Name = syncPair.Name;
            SyncPair.LocalFolderPath = syncPair.LocalFolderPath;
            SyncPair.Mode = syncPair.Mode;
            SyncPair.CompareType = syncPair.CompareType;
            SyncPair.ConflictHandlingType = syncPair.ConflictHandlingType;

            if (!SyncPair.ServerPath.BothNullOrSequenceEqual(syncPair.ServerPath)) SyncPair.ServerPath = syncPair.ServerPath;
            if (!SyncPair.AllowList.BothNullOrSequenceEqual(syncPair.AllowList)) SyncPair.AllowList = syncPair.AllowList;
            if (!SyncPair.DenyList.BothNullOrSequenceEqual(syncPair.DenyList)) SyncPair.AllowList = syncPair.DenyList;
        }

        public void UpdateRun(SyncPairRun run)
        {
            if (Run == null || run == null)
            {
                Run = run;
                return;
            }

            Run.Id = run.Id;
            Run.State = run.State;
            Run.CurrentCount = run.CurrentCount;
            Run.AllFilesCount = run.AllFilesCount;
            Run.ComparedFilesCount = run.ComparedFilesCount;
            Run.EqualFilesCount = run.EqualFilesCount;
            Run.ConflictFilesCount = run.ConflictFilesCount;
            Run.CopiedLocalFilesCount = run.CopiedLocalFilesCount;
            Run.CopiedServerFilesCount = run.CopiedServerFilesCount;
            Run.DeletedLocalFilesCount = run.DeletedLocalFilesCount;
            Run.DeletedServerFilesCount = run.DeletedServerFilesCount;
            Run.ErrorFilesCount = run.ErrorFilesCount;
            Run.IgnoreFilesCount = run.IgnoreFilesCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

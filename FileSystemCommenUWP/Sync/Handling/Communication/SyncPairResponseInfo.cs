using StdOttStandard.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class SyncPairResponseInfo : INotifyPropertyChanged
    {

        private string runToken, currentQueryFolderRelPath;
        private SyncPairHandlerState state;
        private int currentCount, totalCount;
        private Dictionary<string, FilePairInfo> allFiles;
        private ObservableCollection<FilePairInfo> comparedFiles, equalFiles, ignoreFiles, conflictFiles,
            copiedLocalFiles, copiedServerFiles, deletedLocalFiles, deletedServerFiles;
        private ObservableCollection<ErrorFilePairInfo> errorFiles;
        private FilePairInfo? currentCopyToLocalFile, currentCopyToServerFile,
            currentDeleteFromServerFile, currentDeleteFromLocalFile;

        public string RunToken
        {
            get => runToken;
            set
            {
                if (value == runToken) return;

                runToken = value;
                OnPropertyChanged(nameof(RunToken));
            }
        }

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

        public FilePairInfo[] AllFiles
        {
            get => allFiles.Values.ToArray();
            set
            {
                allFiles.Clear();
                foreach (FilePairInfo file in value.ToNotNull())
                {
                    allFiles.Add(file.RelativePath, file);
                }

                OnPropertyChanged(nameof(AllFiles));
            }
        }

        public ObservableCollection<FilePairInfo> ComparedFiles
        {
            get => comparedFiles;
            set
            {
                if (value == comparedFiles) return;

                comparedFiles = value;
                OnPropertyChanged(nameof(ComparedFiles));
            }
        }

        public ObservableCollection<FilePairInfo> EqualFiles
        {
            get => equalFiles;
            set
            {
                if (value == equalFiles) return;

                equalFiles = value;
                OnPropertyChanged(nameof(EqualFiles));
            }
        }

        public ObservableCollection<FilePairInfo> IgnoreFiles
        {
            get => ignoreFiles;
            set
            {
                if (value == ignoreFiles) return;

                ignoreFiles = value;
                OnPropertyChanged(nameof(IgnoreFiles));
            }
        }

        public ObservableCollection<FilePairInfo> ConflictFiles
        {
            get => conflictFiles;
            set
            {
                if (value == conflictFiles) return;

                conflictFiles = value;
                OnPropertyChanged(nameof(ConflictFiles));
            }
        }

        public ObservableCollection<FilePairInfo> CopiedLocalFiles
        {
            get => copiedLocalFiles;
            set
            {
                if (value == copiedLocalFiles) return;

                copiedLocalFiles = value;
                OnPropertyChanged(nameof(CopiedLocalFiles));
            }
        }

        public ObservableCollection<FilePairInfo> CopiedServerFiles
        {
            get => copiedServerFiles;
            set
            {
                if (value == copiedServerFiles) return;

                copiedServerFiles = value;
                OnPropertyChanged(nameof(CopiedServerFiles));
            }
        }

        public ObservableCollection<FilePairInfo> DeletedLocalFiles
        {
            get => deletedLocalFiles;
            set
            {
                if (value == deletedLocalFiles) return;

                deletedLocalFiles = value;
                OnPropertyChanged(nameof(DeletedLocalFiles));
            }
        }

        public ObservableCollection<FilePairInfo> DeletedServerFiles
        {
            get => deletedServerFiles;
            set
            {
                if (value == deletedServerFiles) return;

                deletedServerFiles = value;
                OnPropertyChanged(nameof(DeletedServerFiles));
            }
        }

        public ObservableCollection<ErrorFilePairInfo> ErrorFiles
        {
            get => errorFiles;
            set
            {
                if (value == errorFiles) return;

                errorFiles = value;
                OnPropertyChanged(nameof(ErrorFiles));
            }
        }

        public string CurrentQueryFolderRelPath
        {
            get => currentQueryFolderRelPath;
            set
            {
                if (value == currentQueryFolderRelPath) return;

                currentQueryFolderRelPath = value;
                OnPropertyChanged(nameof(CurrentQueryFolderRelPath));
            }
        }

        public FilePairInfo? CurrentCopyToLocalFile
        {
            get => currentCopyToLocalFile;
            set
            {
                if (Equals(value, currentCopyToLocalFile)) return;

                currentCopyToLocalFile = value;
                OnPropertyChanged(nameof(CurrentCopyToLocalFile));
            }
        }

        public FilePairInfo? CurrentCopyToServerFile
        {
            get => currentCopyToServerFile;
            set
            {
                if (Equals(value, currentCopyToServerFile)) return;

                currentCopyToServerFile = value;
                OnPropertyChanged(nameof(CurrentCopyToServerFile));
            }
        }

        public FilePairInfo? CurrentDeleteFromServerFile
        {
            get => currentDeleteFromServerFile;
            set
            {
                if (Equals(value, currentDeleteFromServerFile)) return;

                currentDeleteFromServerFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromServerFile));
            }
        }

        public FilePairInfo? CurrentDeleteFromLocalFile
        {
            get => currentDeleteFromLocalFile;
            set
            {
                if (Equals(value, currentDeleteFromLocalFile)) return;

                currentDeleteFromLocalFile = value;
                OnPropertyChanged(nameof(CurrentDeleteFromLocalFile));
            }
        }

        public SyncPairResponseInfo()
        {
            currentCount = 0;
            totalCount = 0;
            allFiles = new Dictionary<string, FilePairInfo>();
            comparedFiles = new ObservableCollection<FilePairInfo>();
            equalFiles = new ObservableCollection<FilePairInfo>();
            ignoreFiles = new ObservableCollection<FilePairInfo>();
            conflictFiles = new ObservableCollection<FilePairInfo>();
            copiedLocalFiles = new ObservableCollection<FilePairInfo>();
            copiedServerFiles = new ObservableCollection<FilePairInfo>();
            deletedLocalFiles = new ObservableCollection<FilePairInfo>();
            deletedServerFiles = new ObservableCollection<FilePairInfo>();
            errorFiles = new ObservableCollection<ErrorFilePairInfo>();
            currentQueryFolderRelPath = null;
            currentCopyToLocalFile = null;
            currentCopyToServerFile = null;
            currentDeleteFromServerFile = null;
            currentDeleteFromLocalFile = null;
        }

        public void AddFile(FilePairInfo file)
        {
            allFiles[file.RelativePath] = file;
        }

        public bool TryGetFile(string key, out FilePairInfo file)
        {
            return allFiles.TryGetValue(key, out file);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairEdit : TaskCompletionSource<bool>
    {
        private StorageFolder localFolder;

        public bool IsAdd { get; }

        public SyncPair Sync { get; }

        public StorageFolder LocalFolder
        {
            get => localFolder;
            set
            {
                localFolder = value;
                Sync.LocalFolderPath = localFolder?.Path;
            }
        }

        public Api Api { get; }

        public SyncPairEdit(SyncPair sync, StorageFolder localFolder, Api api, bool isAdd)
        {
            Sync = sync;
            LocalFolder = localFolder;
            Api = api;
            IsAdd = isAdd;
        }
    }
}

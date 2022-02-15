using FileSystemUWP.API;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairEdit : TaskCompletionSource<bool>
    {
        public bool IsAdd { get; }

        public SyncPair Sync { get; }

        public Api Api { get; }

        public SyncPairEdit(SyncPair sync, Api api, bool isAdd)
        {
            Sync = sync;
            Api = api;
            IsAdd = isAdd;
        }
    }
}

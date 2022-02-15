using FileSystemUWP.API;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairEdit : TaskCompletionSource<bool>
    {
        public SyncPair Sync { get; }

        public Api Api { get; }

        public SyncPairEdit(SyncPair sync, Api api)
        {
            Sync = sync;
            Api = api;
        }
    }
}

using StdOttStandard.AsyncResult;

namespace FileSystemUWP.Sync.Definitions
{
    class SyncPairEdit : AsyncResult<bool>
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

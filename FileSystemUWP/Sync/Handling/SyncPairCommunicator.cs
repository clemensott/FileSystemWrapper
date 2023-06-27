using FileSystemCommonUWP.Sync.Handling.Communication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Handling
{
    class SyncPairCommunicator
    {
        private readonly Dictionary<string, SyncPairForegroundContainer> communicators;

        public event EventHandler<SyncPairForegroundContainer> CurrentSyncChanged;

        public Queue<string> Queue { get; }

        public SyncPairCommunicator()
        {
            communicators = new Dictionary<string, SyncPairForegroundContainer>();
            Queue = new Queue<string>();
        }

        public bool TryGetContainer(string runToken, out SyncPairForegroundContainer container)
        {
            return communicators.TryGetValue(runToken, out container);
        }

        public void Enqueue(SyncPairForegroundContainer container)
        {
            Queue.Enqueue(container.Request.RunToken);
            communicators[container.Request.RunToken] = container;
        }

        public async Task Cancel(string runToken)
        {

        }
    }
}

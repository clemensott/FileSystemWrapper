using StdOttStandard.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.AccessCache;

namespace FileSystemCommonUWP.Sync.Definitions
{
    public class SyncPairs : ObservableCollection<SyncPair>
    {
        private string[] loadedTokens;

        public SyncPairs()
        {
        }

        public SyncPairs(IEnumerable<SyncPair> pairs) : base(pairs)
        {
            SaveLocalFolders();
        }

        public void SaveLocalFolders()
        {
            foreach (SyncPair pair in this)
            {
                //pair.SaveLocalFolder();
            }

            foreach (string loadedToken in loadedTokens.ToNotNull())
            {
                //if (this.All(p => p.Token != loadedToken))
                //{
                //    RemoveFromFutureAccessList(loadedToken);
                //}
            }

            //loadedTokens = this.Select(p => p.Token).ToArray();
        }

        private static void RemoveFromFutureAccessList(string token)
        {
            try
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(token);
                }
            }
            catch { }
        }
    }
}

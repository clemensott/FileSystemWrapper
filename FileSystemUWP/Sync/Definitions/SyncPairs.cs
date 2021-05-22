using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FileSystemUWP.Sync.Definitions
{
    public class SyncPairs : ObservableCollection<SyncPair>
    {
        public SyncPairs() : base() { }

        protected override void ClearItems()
        {
            foreach (SyncPair sync in this)
            {
                OnRemove(sync);
            }

            base.ClearItems();
        }

        protected override async void InsertItem(int index, SyncPair item)
        {
            base.InsertItem(index, item);

            await OnAdd(item);
        }

        protected override void RemoveItem(int index)
        {
            OnRemove(this[index]);

            base.RemoveItem(index);
        }

        protected override async void SetItem(int index, SyncPair item)
        {
            OnRemove(this[index]);

            base.SetItem(index, item);

            await OnAdd(item);
        }

        private static async Task OnAdd(SyncPair sync)
        {
            if (sync == null) return;

            sync.PropertyChanged += Item_PropertyChanged;

            if (sync.LocalFolder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(sync.Token, sync.LocalFolder);
            }
            else
            {
                sync.LocalFolder = await TryGetFolderFromFutureAccessList(sync);
            }
        }

        private static void OnRemove(SyncPair sync)
        {
            if (sync == null) return;

            sync.PropertyChanged -= Item_PropertyChanged;

            RemoveFromFutureAccessList(sync);
        }

        private static void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SyncPair.LocalFolder))
            {
                SyncPair sync = (SyncPair)sender;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(sync.Token, sync.LocalFolder);
            }
        }

        private static void RemoveFromFutureAccessList(SyncPair sync)
        {
            try
            {
                if (StorageApplicationPermissions.FutureAccessList.Entries.Any(e => e.Token == sync.Token))
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(sync.Token);
                }
            }
            catch { }
        }

        private static async Task<StorageFolder> TryGetFolderFromFutureAccessList(SyncPair sync)
        {
            try
            {
                return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(sync.Token);
            }
            catch
            {
                return null;
            }
        }
    }
}

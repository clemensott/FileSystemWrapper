using StdOttStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Result
{
    public class SyncedItems
    {
        private readonly IDictionary<string, SyncedItem> lastItems;
        private readonly IList<SyncedItem> newResult;

        public string Token { get; }

        private SyncedItems(string token, IDictionary<string, SyncedItem> lastItems)
        {
            Token = token;
            this.lastItems = lastItems;
            newResult = new List<SyncedItem>();
        }

        public static async Task<SyncedItems> Create(string token)
        {
            IDictionary<string, SyncedItem> lastItems = await GetLastItems(token);
            return new SyncedItems(token, lastItems);
        }

        private static async Task<IDictionary<string, SyncedItem>> GetLastItems(string token)
        {
            IStorageItem item = await ApplicationData.Current.LocalFolder.TryGetItemAsync($"result_{token}.xml");
            if (item == null) return new Dictionary<string, SyncedItem>();
            if (!item.IsOfType(StorageItemTypes.File))
            {
                throw new Exception("Storage item at result path is not a file");
            }

            try
            {
                string xml = await FileIO.ReadTextAsync((IStorageFile)item);
                SyncedItem[] syncedItems = StdUtils.XmlDeserializeText<SyncedItem[]>(xml);
                return syncedItems.Where(r => !string.IsNullOrWhiteSpace(r.RelativePath)).ToDictionary(r => r.RelativePath);
            }
            catch
            {
                return new Dictionary<string, SyncedItem>();
            }
        }

        public bool TryGetItem(string relativePath, out SyncedItem last)
        {
            return lastItems.TryGetValue(relativePath, out last);
        }

        public void Add(SyncedItem syncedItem)
        {
            newResult.Add(syncedItem);
        }

        public async Task SaveNewResult()
        {
            string xml = StdUtils.XmlSerialize(newResult.ToArray());
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"result_{Token}.xml", CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(file, xml);
        }
    }
}

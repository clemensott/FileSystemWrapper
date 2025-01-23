using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FileSystemCommonUWP.Sync
{
    public class SyncLocalFolderHelper
    {
        public static async Task<StorageFolder> GetLocalFolder(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
            {
                throw new Exception("Local folder not found for requested token");
            }

            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        public static void SaveLocalFolder(string token, StorageFolder folder)
        {
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
            }
            else
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
}

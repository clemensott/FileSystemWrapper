using FileSystemCommon;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    class HashComparer : BaseSyncFileComparer
    {
        private readonly int? partialSize;

        public HashComparer(Api api, int? partialSize = null) : base(api, partialSize.HasValue ? SyncCompareType.PartialHash : SyncCompareType.Hash)
        {
            this.partialSize = partialSize;
        }

        public override bool EqualsValue(object obj1, object obj2)
        {
            return obj1 is string value1 && obj2 is string value2 && value1 == value2;
        }

        public override Task<object> GetLocalCompareValue(StorageFile localFile)
        {
            return GetFileHash(localFile, partialSize);
        }

        private static async Task<object> GetFileHash(StorageFile file, int? partialSize)
        {
            using (SHA1 hashing = SHA1.Create())
            {
                byte[] hashBytes;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    if (partialSize > 0)
                    {
                        byte[] partialData = await Utils.GetPartialBinary(stream, partialSize.Value);
                        hashBytes = hashing.ComputeHash(partialData);
                    }
                    else hashBytes = hashing.ComputeHash(stream);
                }

                return Convert.ToBase64String(hashBytes);
            }
        }

        public override async Task<object> GetServerCompareValue(string serverFilePath)
        {
            return await api.GetFileHash(serverFilePath, partialSize);
        }

        public override async Task GetServerCompareValues(string[] serverFilePaths, Func<string, object, string, Task> onValueAction)
        {
            await api.GetFilesHash(serverFilePaths, partialSize, item => onValueAction(item.FilePath, item.Hash, item.ErrorMessage));
        }
    }
}

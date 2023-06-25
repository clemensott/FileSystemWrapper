using FileSystemCommon;
using FileSystemUWP.API;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemUWP.Sync.Handling.CompareType
{
    class HashComparer : ISyncFileComparer
    {
        private readonly int? partialSize;

        public bool IsPartial => partialSize.HasValue;

        public HashComparer(int? partialSize = null)
        {
            this.partialSize = partialSize;
        }

        public new bool Equals(object obj1, object obj2)
        {
            return obj1 is string value1 && obj2 is string value2 && value1 == value2;
        }

        public Task<object> GetLocalCompareValue(StorageFile localFile)
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

        public async Task<object> GetServerCompareValue(string serverFilePath, Api api)
        {
            return await api.GetFileHash(serverFilePath, partialSize);
        }
    }
}

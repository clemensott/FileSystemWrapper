using FileSystemCommon.Models.FileSystem;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileSystemUWP.Sync.Handling.CompareType
{
    class SizeComparer : ISyncFileComparer
    {
        public new bool Equals(object obj1, object obj2)
        {
            return obj1 is long value1 && obj2 is long value2 && value1 == value2;
        }

        public async Task<object> GetLocalCompareValue(StorageFile localFile)
        {
            BasicProperties props = await localFile.GetBasicPropertiesAsync();
            return (long) props.Size;
        }

        public async Task<object> GetServerCompareValue(string serverFilePath, Api api)
        {
            FileItemInfo props = await api.GetFileInfo(serverFilePath);
            return props.Size;
        }
    }
}

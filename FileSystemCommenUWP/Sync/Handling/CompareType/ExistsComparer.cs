using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    class ExistsComparer : ISyncFileComparer
    {
        public SyncCompareType Type => SyncCompareType.Exists;

        public new bool Equals(object obj1, object obj2)
        {
            return obj1 is bool value1 && obj2 is bool value2 && value1 == value2;
        }

        public Task<object> GetLocalCompareValue(StorageFile localFile)
        {
            return Task.FromResult<object>(localFile != null);
        }

        public async Task<object> GetServerCompareValue(string serverFilePath, Api api)
        {
            return await api.FileExists(serverFilePath);
        }
    }
}

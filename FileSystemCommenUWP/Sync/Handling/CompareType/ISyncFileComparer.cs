using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    public interface ISyncFileComparer
    {
        SyncCompareType Type { get; }

        Task<object> GetServerCompareValue(string serverFilePath, Api api);

        Task<object> GetLocalCompareValue(StorageFile localFile);

        bool Equals(object obj1, object obj2);
    }
}

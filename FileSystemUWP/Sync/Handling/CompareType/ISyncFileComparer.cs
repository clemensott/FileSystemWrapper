using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemUWP.Sync.Handling.CompareType
{
    public interface ISyncFileComparer
    {
        Task<object> GetServerCompareValue(string serverFilePath, Api api);

        Task<object> GetLocalCompareValue(StorageFile localFile);

        bool Equals(object obj1, object obj2);
    }
}

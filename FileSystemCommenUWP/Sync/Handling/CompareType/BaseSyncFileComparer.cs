using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    public abstract class BaseSyncFileComparer
    {
        protected readonly Api api;

        public SyncCompareType Type { get; }

        protected BaseSyncFileComparer(Api api, SyncCompareType type)
        {
            this.api = api;
            Type = type;
        }

        public abstract Task<object> GetServerCompareValue(string serverFilePath);

        public abstract Task GetServerCompareValues(string[] serverFilePaths, Func<string, object, string, Task> onValueAction);

        public abstract Task<object> GetLocalCompareValue(StorageFile localFile);

        public abstract bool EqualsValue(object obj1, object obj2);
    }
}

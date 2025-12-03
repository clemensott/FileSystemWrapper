using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.API;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    class ExistsComparer : BaseSyncFileComparer
    {
        public ExistsComparer(Api api) : base(api, SyncCompareType.Exists) { }

        public override bool EqualsValue(object obj1, object obj2)
        {
            return obj1 is bool value1 && obj2 is bool value2 && value1 == value2;
        }

        public override Task<object> GetLocalCompareValue(StorageFile localFile)
        {
            return Task.FromResult<object>(localFile != null);
        }

        public override async Task<object> GetServerCompareValue(string serverFilePath)
        {
            return await api.FileExists(serverFilePath);
        }

        public override async Task GetServerCompareValues(string[] serverFilePaths, Func<string, object, string, Task> onValueAction)
        {
            await api.GetFilesExits(serverFilePaths, item => onValueAction(item.FilePath, item.Exists, item.ErrorMessage));
        }
    }
}

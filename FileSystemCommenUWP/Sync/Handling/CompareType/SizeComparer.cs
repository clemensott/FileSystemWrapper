using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.API;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileSystemCommonUWP.Sync.Handling.CompareType
{
    class SizeComparer : BaseSyncFileComparer
    {
        public SizeComparer(Api api) : base(api, SyncCompareType.Size) { }

        public override bool EqualsValue(object obj1, object obj2)
        {
            return obj1 is long value1 && obj2 is long value2 && value1 == value2;
        }

        public override async Task<object> GetLocalCompareValue(StorageFile localFile)
        {
            BasicProperties props = await localFile.GetBasicPropertiesAsync();
            return (long)props.Size;
        }

        public override async Task<object> GetServerCompareValue(string serverFilePath)
        {
            FileItemInfo props = await api.GetFileInfo(serverFilePath);
            return props.Size;
        }

        public override async Task GetServerCompareValues(string[] serverFilePaths, Func<string, object, string, Task> onValueAction)
        {
            await api.GetFilesInfo(serverFilePaths, item => onValueAction(item.FilePath, item.Info?.Size, item.ErrorMessage));
        }
    }
}

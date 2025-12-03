using System.Text.Json;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.CompareType;

class SizeComparer(Api api) : BaseSyncFileComparer(api, SyncCompareType.Size)
{
    private static bool TryParseValueObject(object? obj, out long value)
    {
        switch (obj)
        {
            case long val:
                value = val;
                return true;

            case JsonElement jsonElement when jsonElement.TryGetInt64(out value):
                return true;
            
            default:
                value = 0;
                return false;
        }
    }

    public override bool EqualsValue(object? obj1, object? obj2)
    {
        return TryParseValueObject(obj1, out long value1) && 
               TryParseValueObject(obj2, out long value2) &&
               value1 == value2;
    }

    public override Task<object> GetLocalCompareValue(string localFilePath)
    {
        FileInfo fileInfo = new FileInfo(localFilePath);
        return Task.FromResult<object>(fileInfo.Length);
    }

    public override async Task<object> GetServerCompareValue(string serverFilePath)
    {
        FileItemInfo props = await api.GetFileInfo(serverFilePath);
        return props.Size;
    }

    public override async Task GetServerCompareValues(string[] serverFilePaths,
        Func<string, object?, string, Task> onValueAction)
    {
        await api.GetFilesInfo(serverFilePaths,
            item => onValueAction(item.FilePath, item.Info?.Size, item.ErrorMessage));
    }
}
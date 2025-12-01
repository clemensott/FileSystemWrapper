using System.Text.Json;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.CompareType;

class ExistsComparer(Api api) : BaseSyncFileComparer(api, SyncCompareType.Exists)
{
    private static bool TryParseValueObject(object? obj, out bool value)
    {
        switch (obj)
        {
            case bool val:
                value = val;
                return true;

            case JsonElement { ValueKind: JsonValueKind.True } jsonElement:
                value = true;
                return true;

            case JsonElement { ValueKind: JsonValueKind.False } jsonElement:
                value = false;
                return true;

            default:
                value = false;
                return false;
        }
    }

    public override bool EqualsValue(object? obj1, object? obj2)
    {
        return TryParseValueObject(obj1, out bool value1) && 
               TryParseValueObject(obj2, out bool value2) &&
               value1 == value2;
    }

    public override Task<object> GetLocalCompareValue(string localFilePath)
    {
        return Task.FromResult<object>(File.Exists(localFilePath));
    }

    public override async Task<object> GetServerCompareValue(string serverFilePath)
    {
        return await api.FileExists(serverFilePath);
    }

    public override async Task GetServerCompareValues(string[] serverFilePaths,
        Func<string, object?, string, Task> onValueAction)
    {
        await api.GetFilesExits(serverFilePaths, item => onValueAction(item.FilePath, item.Exists, item.ErrorMessage));
    }
}
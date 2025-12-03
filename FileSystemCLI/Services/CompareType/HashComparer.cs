using FileSystemCommon;
using System.Security.Cryptography;
using System.Text.Json;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.CompareType;

class HashComparer(Api api, int? partialSize = null) : BaseSyncFileComparer(api,
    partialSize.HasValue ? SyncCompareType.PartialHash : SyncCompareType.Hash)
{
    private static bool TryParseValueObject(object? obj, out string? value)
    {
        switch (obj)
        {
            case string val:
                value = val;
                return true;

            case JsonElement { ValueKind: JsonValueKind.String  } jsonElement:
                value = jsonElement.GetString();
                return true;

            default:
                value = null;
                return false;
        }
    }

    public override bool EqualsValue(object? obj1, object? obj2)
    {
        return TryParseValueObject(obj1, out string? value1) && 
               TryParseValueObject(obj2, out string? value2) &&
               value1 == value2;
    }

    public override Task<object> GetLocalCompareValue(string localFilePath)
    {
        return GetFileHash(localFilePath, partialSize);
    }

    private static async Task<object> GetFileHash(string localFilePath, int? partialSize)
    {
        byte[] hashBytes;
        
        using SHA1 hashing = SHA1.Create();
        await using (Stream stream = File.OpenRead(localFilePath))
        {
            if (partialSize > 0)
            {
                byte[] partialData = await Utils.GetPartialBinary(stream, partialSize.Value);
                hashBytes = hashing.ComputeHash(partialData);
            }
            else hashBytes = await hashing.ComputeHashAsync(stream);
        }

        return Convert.ToBase64String(hashBytes);
    }

    public override async Task<object> GetServerCompareValue(string serverFilePath)
    {
        return await api.GetFileHash(serverFilePath, partialSize);
    }

    public override async Task GetServerCompareValues(string[] serverFilePaths, Func<string, object?, string, Task> onValueAction)
    {
        await api.GetFilesHash(serverFilePaths, partialSize, item => onValueAction(item.FilePath, item.Hash, item.ErrorMessage));
    }
}
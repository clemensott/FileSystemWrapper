using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.CompareType;

public abstract class BaseSyncFileComparer(Api api, SyncCompareType type)
{
    protected readonly Api api = api;

    public SyncCompareType Type { get; } = type;

    public abstract Task<object> GetServerCompareValue(string serverFilePath);

    public abstract Task GetServerCompareValues(string[] serverFilePaths, Func<string, object?, string, Task> onValueAction);

    public abstract Task<object> GetLocalCompareValue(string localFilePath);

    public abstract bool EqualsValue(object? obj1, object? obj2);
}
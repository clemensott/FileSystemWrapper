using FileSystemCommon.Models.Sync.Handling;

namespace FileSystemCLI.Models;

public struct SyncPairStateFileModel: IBaseSyncPairStateFile
{
    public string RelativePath { get; set; }

    public object? LocalCompareValue { get; set; }

    public object? ServerCompareValue { get; set; }
}
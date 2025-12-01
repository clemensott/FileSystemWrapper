namespace FileSystemCLI.Models;

public struct SyncPairStateFileModel
{
    public string RelativePath { get; set; }

    public object? LocalCompareValue { get; set; }

    public object? ServerCompareValue { get; set; }
}
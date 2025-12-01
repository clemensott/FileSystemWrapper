namespace FileSystemCLI.Models;

public class FilePairModel
{
    public string RelativePath { get; init; }

    public string ServerFullPath { get; init; }

    public bool ServerFileExists { get; init; }

    public string LocalFilePath { get; init; }

    public bool LocalFileExists { get; init; }

    public object? ServerCompareValue { get; set; }

    public object? LocalCompareValue { get; set; }

    public SyncPairStateFileModel ToState()
    {
        return new SyncPairStateFileModel()
        {
            RelativePath = RelativePath,
            LocalCompareValue = LocalCompareValue,
            ServerCompareValue = ServerCompareValue,
        };
    }
}
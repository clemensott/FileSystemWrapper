namespace FileSystemCLI.Models;

public class SyncPairStateModel
{
    public SyncPairStateFileModel[] Files { get; set; }
    
    public DateTime LastFullSync { get; set; }
    
    public DateTime LastServerChangeSync { get; set; }
}
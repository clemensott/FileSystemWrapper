namespace FileSystemCLI.Models;

public class SyncPairConfigModel
{
    public bool WithSubfolders { get; init; }
    
    public string LocalFolderPath { get; init; }
    
    public string ServerFolderPath { get; init; }
    
    public string Mode { get; init; }
    
    public string CompareType { get; init; }
    
    public string ConflictHandling { get; init; }
    
    public string[]? AllowList { get; init; }
    
    public string[]? DenyList { get; init; }
    
    public string? StateFilePath { get; init; }
}
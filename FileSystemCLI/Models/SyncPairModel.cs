using FileSystemCLI.Services;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Models;

public class SyncPairModel
{
    public bool WithSubfolders { get; init; }
    
    public string LocalFolderPath { get; init; }
    
    public string ServerFolderPath { get; init; }
    
    public SyncMode Mode { get; init; }
    
    public SyncCompareType CompareType { get; init; }
    
    public SyncConflictHandlingType ConflictHandling { get; init; }
    
    public string[]? AllowList { get; init; }
    
    public string[]? DenyList { get; init; }
    
    public string? StateFilePath { get; init; }
    
    public TimeSpan FullSyncInterval { get; init; }
    
    public TimeSpan ServerFetchChangesInterval { get; init; }
}
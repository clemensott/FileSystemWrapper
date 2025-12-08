using System.Text.Json;
using FileSystemCommon.Models.Sync.Handling;
using StdOttStandard.Linq;

namespace FileSystemCLI.Models;

public class SyncPairState : BaseSyncPairState<SyncPairStateFileModel>
{
    public DateTime LastFullSync { get; set; }
    
    public DateTime LastServerChangeSync { get; set; }
    
    public SyncPairState()
    {
    }

    public SyncPairState(IEnumerable<SyncPairStateFileModel> files) : base(files)
    {
    }

    public static async Task<SyncPairState> LoadSyncPairState(string? stateFilePath)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath) || !File.Exists(stateFilePath)) return new SyncPairState();

        string json = await File.ReadAllTextAsync(stateFilePath);
        SyncPairStateModel? state = JsonSerializer.Deserialize<SyncPairStateModel>(json);
        if (state is null) throw new Exception("State files are missing");

        return new SyncPairState(state.Files.ToNotNull())
        {
            LastFullSync = state.LastFullSync,
            LastServerChangeSync = state.LastServerChangeSync,
        };
    }
    
    public async Task WriteSyncPairState(string? stateFilePath)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath)) return;

        string json = JsonSerializer.Serialize(new  SyncPairStateModel()
        {
            Files = this.ToArray(),
            LastFullSync = LastFullSync,
            LastServerChangeSync = LastServerChangeSync,
        });
        await File.WriteAllTextAsync(stateFilePath, json);
    }
}
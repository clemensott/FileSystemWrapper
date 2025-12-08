using System.Text.Json;
using FileSystemCommon.Models.Sync.Handling;
using StdOttStandard.Linq;

namespace FileSystemCLI.Models;

public class SyncPairState : BaseSyncPairState<SyncPairStateFileModel>
{
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
        SyncPairStateFileModel[]? files = JsonSerializer.Deserialize<SyncPairStateFileModel[]>(json);
        if (files is null) throw new Exception("State files are missing");

        return new SyncPairState(files.ToNotNull());
    }
    
    public async Task WriteSyncPairState(string? stateFilePath)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath)) return;

        string json = JsonSerializer.Serialize(this.ToArray());
        await File.WriteAllTextAsync(stateFilePath, json);
    }
}
using System.Text.Json;
using FileSystemCLI.Models;
using FileSystemCLI.ProgramArguments;
using FileSystemCLI.Services;
using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.Linq;

namespace FileSystemCLI;

sealed class Program
{
    private static SyncPairsModel? LoadSyncPairFromFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return null;

        string json;
        try
        {
            json = File.ReadAllText(filePath);
        }
        catch (FileNotFoundException)
        {
            throw new Exception($"File for sync config {filePath} not found");
        }

        SyncPairConfigsModel? config;
        try
        {
            config = JsonSerializer.Deserialize<SyncPairConfigsModel>(json);
            if (config is null) throw new Exception("Invalid sync pair config file");
        }
        catch (Exception e)
        {
            throw new Exception("Parsing sync config file failed", e);
        }

        return new SyncPairsModel()
        {
            Pairs = config.Configs.Select(entry => new SyncPairModel()
            {
                WithSubfolders = entry.WithSubfolders,
                LocalFolderPath = entry.LocalFolderPath ?? throw new Exception("No LocalFolderPath specified"),
                ServerFolderPath = entry.ServerFolderPath ?? throw new Exception("No ServerFolderPath specified"),
                Mode = (SyncMode)Enum.Parse(typeof(SyncMode), entry.Mode ?? throw new Exception("No Mode specified")),
                CompareType = (SyncCompareType)Enum.Parse(typeof(SyncCompareType),
                    entry.CompareType ?? throw new Exception("No CompareType specified")),
                ConflictHandling = (SyncConflictHandlingType)Enum.Parse(typeof(SyncConflictHandlingType),
                    entry.ConflictHandling ?? throw new Exception("No ConflictHandling specified")),
                AllowList = entry.AllowList,
                DenyList = entry.DenyList,
                StateFilePath = entry.StateFilePath,
            }).ToArray(),
            Api = config.Api ?? throw new Exception("No Api specified"),
        };
    }

    private static string? GetStateFilePath(SyncMode? mode, string? stateFilePath)
    {
        if (mode == SyncMode.TwoWay && string.IsNullOrWhiteSpace(stateFilePath))
        {
            throw new Exception("State file path cannot be empty for two way mode");
        }

        return stateFilePath;
    }

    private static SyncPairsModel GetSyncPair(ParsedArgs args)
    {
        SyncPairsModel? fileSyncPairs = LoadSyncPairFromFile(args.ConfigFilePath);

        SyncPairModel?[] pairs = fileSyncPairs?.Pairs ?? new SyncPairModel?[] { null }; 
        return new SyncPairsModel()
        {
            Api = new ApiModel()
            {
                BaseUrl = args.ServerBaseUrl ??
                          fileSyncPairs?.Api.BaseUrl ?? throw new Exception("No BaseUrl specified"),
                Username = args.ServerUsername ??
                           fileSyncPairs?.Api.Username ?? throw new Exception("No Username specified"),
                Password = args.ServerPassword ??
                           fileSyncPairs?.Api.Password ?? throw new Exception("No Username specified"),
            },
            Pairs = pairs.Select(pair =>
            {
                SyncMode? mode = args.Mode ?? pair?.Mode;
                return new SyncPairModel()
                {
                    WithSubfolders = args.WithSubfolders ??
                                     pair?.WithSubfolders ?? throw new Exception("No WithSubfolders specified"),
                    LocalFolderPath = args.LocalFolderPath ??
                                      pair?.LocalFolderPath ?? throw new Exception("No LocalFolderPath specified"),
                    ServerFolderPath = args.ServerFolderPath ??
                                       pair?.ServerFolderPath ?? throw new Exception("No ServerFolderPath specified"),
                    Mode = mode ?? throw new Exception("No Mode specified"),
                    CompareType = args.CompareType ??
                                  pair?.CompareType ?? throw new Exception("No CompareType specified"),
                    ConflictHandling = args.ConflictHandling ??
                                       pair?.ConflictHandling ?? throw new Exception("No ConflictHandling specified"),
                    AllowList = args.AllowList ??
                                pair?.AllowList,
                    DenyList = args.DenyList ??
                               pair?.DenyList,
                    StateFilePath = GetStateFilePath(mode, args.StateFilePath ?? pair?.StateFilePath),
                };
            }).ToArray(),
        };
    }

    private static SyncPairState LoadSyncPairState(string? stateFilePath)
    {
        SyncPairState state = new SyncPairState();
        if (string.IsNullOrWhiteSpace(stateFilePath) || !File.Exists(stateFilePath)) return state;

        string json = File.ReadAllText(stateFilePath);
        SyncPairStateFileModel[]? files = JsonSerializer.Deserialize<SyncPairStateFileModel[]>(json);
        if (files is null) throw new Exception("State files are missing");

        foreach (SyncPairStateFileModel file in files.ToNotNull()) state.AddFile(file);

        return state;
    }

    private static void WriteSyncPairState(string? stateFilePath, SyncPairState state)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath)) return;

        string json = JsonSerializer.Serialize(state.ToArray());
        File.WriteAllText(stateFilePath, json);
    }

    public static async Task Main(string[] args)
    {
        ParsedArgs parsedArgs = ParsedArgs.Parse(args);

        SyncPairsModel syncPairs = GetSyncPair(parsedArgs);

        using Api api = new(syncPairs.Api.BaseUrl, syncPairs.Api.Username, syncPairs.Api.Password);
        if (!await api.Ping())
        {
            Console.WriteLine($"Could not reach server: {api.BaseUrl}");
            return;
        }

        if (!await api.Login())
        {
            Console.WriteLine($"Could not login at server: {api.BaseUrl}");
            return;
        }

        if (!await api.IsAuthorized())
        {
            Console.WriteLine($"Could authenticate at server: {api.BaseUrl}");
            return;
        }

        await api.LoadConfig();

        SyncPairHandler? handler = null;
        Console.CancelKeyPress += (_, e) =>
        {
            SyncPairHandler? currentHandler = handler;
            if (currentHandler is null || currentHandler.IsCancelled) return;

            Console.WriteLine("Cancel sync!");
            currentHandler.Cancel();
            e.Cancel = true;
        };

        foreach (SyncPairModel syncPair in syncPairs.Pairs)
        {
            SyncPairState lastSyncPairState = LoadSyncPairState(syncPair.StateFilePath);

            handler = new SyncPairHandler(parsedArgs.IsTestRun, syncPair, api, lastSyncPairState);
            await handler.Run();

            if (handler.IsCancelled) return;

            if (!parsedArgs.IsTestRun) WriteSyncPairState(syncPair.StateFilePath, handler.CurrentState);
        }

        Console.WriteLine("Sync finished successfully!!");
    }
}
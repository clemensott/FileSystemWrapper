using System.Text.Json;
using FileSystemCLI.Models;
using FileSystemCLI.ProgramArguments;
using FileSystemCLI.Services;
using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.Linq;

namespace FileSystemCLI;

sealed class Program
{
    private static SyncPairModel? LoadSyncPairFromFile(string? filePath)
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

        SyncPairConfigModel? pair;
        try
        {
            pair = JsonSerializer.Deserialize<SyncPairConfigModel>(json);
            if (pair is null) throw new Exception("Invalid sync pair config file");
        }
        catch (Exception e)
        {
            throw new Exception("Parsing sync config file failed", e);
        }

        return new SyncPairModel()
        {
            WithSubfolders = pair.WithSubfolders,
            LocalFolderPath = pair.LocalFolderPath ?? throw new Exception("No LocalFolderPath specified"),
            ServerFolderPath = pair.ServerFolderPath ?? throw new Exception("No ServerFolderPath specified"),
            Mode = (SyncMode)Enum.Parse(typeof(SyncMode), pair.Mode ?? throw new Exception("No Mode specified")),
            CompareType = (SyncCompareType)Enum.Parse(typeof(SyncCompareType),
                pair.CompareType ?? throw new Exception("No CompareType specified")),
            ConflictHandling = (SyncConflictHandlingType)Enum.Parse(typeof(SyncConflictHandlingType),
                pair.ConflictHandling ?? throw new Exception("No ConflictHandling specified")),
            AllowList = pair.AllowList,
            DenyList = pair.DenyList,
            StateFilePath = pair.StateFilePath,
            Api = pair.Api ?? throw new Exception("No Api specified"),
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

    private static SyncPairModel GetSyncPair(ParsedArgs args)
    {
        SyncPairModel? fileSyncPair = LoadSyncPairFromFile(args.ConfigFilePath);

        SyncMode? mode = args.Mode ?? fileSyncPair?.Mode;
        return new SyncPairModel()
        {
            Api = new ApiModel()
            {
                BaseUrl = args.ServerBaseUrl ??
                          fileSyncPair?.Api.BaseUrl ?? throw new Exception("No BaseUrl specified"),
                Username = args.ServerUsername ??
                           fileSyncPair?.Api.Username ?? throw new Exception("No Username specified"),
                Password = args.ServerPassword ??
                           fileSyncPair?.Api.Password ?? throw new Exception("No Username specified"),
            },
            WithSubfolders = args.WithSubfolders ??
                             fileSyncPair?.WithSubfolders ?? throw new Exception("No WithSubfolders specified"),
            LocalFolderPath = args.LocalFolderPath ??
                              fileSyncPair?.LocalFolderPath ?? throw new Exception("No LocalFolderPath specified"),
            ServerFolderPath = args.ServerFolderPath ??
                               fileSyncPair?.ServerFolderPath ?? throw new Exception("No ServerFolderPath specified"),
            Mode = mode ?? throw new Exception("No Mode specified"),
            CompareType = args.CompareType ??
                          fileSyncPair?.CompareType ?? throw new Exception("No CompareType specified"),
            ConflictHandling = args.ConflictHandling ??
                               fileSyncPair?.ConflictHandling ?? throw new Exception("No ConflictHandling specified"),
            AllowList = args.AllowList ??
                        fileSyncPair?.AllowList,
            DenyList = args.DenyList ??
                       fileSyncPair?.DenyList,
            StateFilePath = GetStateFilePath(mode, args.StateFilePath ?? fileSyncPair?.StateFilePath),
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

        SyncPairModel syncPair = GetSyncPair(parsedArgs);

        SyncPairState lastSyncPairState = LoadSyncPairState(syncPair.StateFilePath);

        using Api api = new(syncPair.Api.BaseUrl, syncPair.Api.Username, syncPair.Api.Password);
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

        SyncPairHandler handler = new SyncPairHandler(parsedArgs.IsTestRun, syncPair, api, lastSyncPairState);

        Console.CancelKeyPress += (_, e) =>
        {
            if (handler.IsCancelled) return;

            Console.WriteLine("Cancel sync!");
            handler.Cancel();
            e.Cancel = true;
        };

        await handler.Run();

        if (handler.IsCancelled) return;

        if (!parsedArgs.IsTestRun) WriteSyncPairState(syncPair.StateFilePath, handler.CurrentState);

        Console.WriteLine("Sync finished successfully!!");
    }
}
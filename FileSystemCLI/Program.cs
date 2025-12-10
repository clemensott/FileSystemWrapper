using System.Text.Json;
using FileSystemCLI.Models;
using FileSystemCLI.ProgramArguments;
using FileSystemCLI.Services;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI;

sealed class Program
{
    private static TimeSpan ParseTimeSpan(string? raw, TimeSpan defaultValue)
    {
        if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

        TimeSpan value = TimeSpan.Parse(raw);
        if (value <= TimeSpan.Zero) throw new Exception("Invalid TimeSpan specified (must be greater than zero)");

        return value;
    }

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
                FullSyncInterval = ParseTimeSpan(entry.FullSyncInterval, TimeSpan.FromDays(1)),
                ServerFetchChangesInterval = ParseTimeSpan(entry.ServerFetchChangesInterval, TimeSpan.FromHours(1)),
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
                    FullSyncInterval = pair?.FullSyncInterval ?? TimeSpan.FromDays(1),
                    ServerFetchChangesInterval = pair?.ServerFetchChangesInterval ?? TimeSpan.FromHours(1),
                };
            }).ToArray(),
        };
    }

    private static async Task DoWatchSyncs(bool isTestRun, bool initialSync, SyncPairsModel syncPairs, Api api)
    {
        SyncPairHandler? initialHandler = null;
        List<SyncFileWatcher> watchers = new List<SyncFileWatcher>();

        bool isCanceled = false;
        Console.CancelKeyPress += (_, e) =>
        {
            if (isCanceled) return;

            Console.WriteLine("Cancel sync!");
            initialHandler?.Cancel();
            foreach (SyncFileWatcher watcher in watchers) watcher.Stop();
            e.Cancel = true;
        };

        foreach (SyncPairModel syncPair in syncPairs.Pairs)
        {
            SyncPairState syncPairState = await SyncPairState.LoadSyncPairState(syncPair.StateFilePath);
            if (initialSync || syncPairState.LastFullSync + syncPair.FullSyncInterval < DateTime.UtcNow)
            {
                try
                {
                    await api.Ensure();
                    initialHandler = new SyncPairHandler(isTestRun, syncPair, api, syncPairState);
                    await initialHandler.Run();

                    if (initialHandler.IsCancelled) return;

                    syncPairState = initialHandler.CurrentState;
                    if (!isTestRun) await syncPairState.WriteSyncPairState(syncPair.StateFilePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error on initial sync before watch: {e.Message}");
                }
            }

            SyncFileWatcher watcher = new SyncFileWatcher(isTestRun, syncPair, api, syncPairState);
            watchers.Add(watcher);
            watcher.Start();
        }

        await Task.WhenAll(watchers.Select(w => w.AwaitSyncTask()));
    }

    private static async Task DoSingleCompleteSyncs(bool isTestRun, SyncPairsModel syncPairs, Api api)
    {
        await api.Ensure();

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
            SyncPairState lastSyncPairState = await SyncPairState.LoadSyncPairState(syncPair.StateFilePath);
            handler = new SyncPairHandler(isTestRun, syncPair, api, lastSyncPairState);
            await handler.Run();

            if (handler.IsCancelled) return;

            if (!isTestRun) await handler.CurrentState.WriteSyncPairState(syncPair.StateFilePath);
        }

        Console.WriteLine("Sync finished successfully!!");
    }

    public static async Task Main(string[] args)
    {
        ParsedArgs parsedArgs = ParsedArgs.Parse(args);

        SyncPairsModel syncPairs = GetSyncPair(parsedArgs);

        using Api api = new(syncPairs.Api.BaseUrl, syncPairs.Api.Username, syncPairs.Api.Password);

        if (parsedArgs.Watch) await DoWatchSyncs(parsedArgs.IsTestRun, parsedArgs.InitialSync, syncPairs, api);
        else await DoSingleCompleteSyncs(parsedArgs.IsTestRun, syncPairs, api);
    }
}
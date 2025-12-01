using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.CommandlineParser;

namespace FileSystemCLI.ProgramArguments;

public class ParsedArgs
{
    public bool IsTestRun { get; init; }

    public string? ConfigFilePath { get; init; }

    public string? ServerBaseUrl { get; init; }

    public string? ServerUsername { get; init; }

    public string? ServerPassword { get; init; }

    public bool? WithSubfolders { get; init; }

    public string? LocalFolderPath { get; init; }

    public string? ServerFolderPath { get; init; }

    public SyncMode? Mode { get; init; }

    public SyncCompareType? CompareType { get; init; }

    public SyncConflictHandlingType? ConflictHandling { get; init; }

    public string[]? AllowList { get; init; }

    public string[]? DenyList { get; init; }

    public string? StateFilePath { get; init; }

    public static ParsedArgs Parse(string[] args)
    {
        Option isTestRunOption = new Option("t", "test-run", "Do a test run that doesn't change anything", false, 0, 0);
        Option configFilePathOption =
            new Option("c", "config-file", "Uses the config from file at given path", false, 1, 1);

        Option serverUrlOption = new Option("url", "server-url", "URL of server", false, 1, 1);
        Option serverUsernameOption = new Option("u", "username", "Username at server", false, 1, 1);
        Option serverPasswordOption = new Option("p", "password", "Password for user", false, 1, 1);

        Option withSubfoldersOption = new Option("s", "with-sub-folders", "Include sub folders in sync", false, 1, 0);
        Option localFolderPathOption = new Option("l", "local-folder", "Path to local folder to sync", false, 1, 1);
        Option serverFolderPathOption = new Option("r", "remote-folder", "Path to remote folder to sync", false, 1, 1);
        Option modeOption = new Option("m", "mode", "Mode to use for sync", false, 1, 1);
        Option compareTypeOption = new Option("c", "compare", "Method to compare the files", false, 1, 1);
        Option conflictHandlingOption = new Option("conflict", "conflict", "Method to handle a conflict", false, 1, 1);
        Option allowListOption = new Option("a", "allow", "List of file endings to allow", false, -1, 0);
        Option denyListsOption = new Option("d", "deny", "List of file endings to deny", false, -1, 0);
        Option stateFilePathOption = new Option("state", "state-file",
            "File to save synced state in and only needed for two way mode", false, 1, 1);

        Options options = new Options(isTestRunOption, configFilePathOption, serverUrlOption, serverUsernameOption,
            serverPasswordOption, withSubfoldersOption, localFolderPathOption, serverFolderPathOption, modeOption,
            compareTypeOption, conflictHandlingOption, allowListOption, denyListsOption, stateFilePathOption);

        OptionParseResult result = options.Parse(args)!;
        OptionParsed parsed;

        return new ParsedArgs()
        {
            IsTestRun = result.HasValidOptionParseds(isTestRunOption),
            ConfigFilePath = result.TryGetFirstValidOptionParseds(configFilePathOption, out parsed)
                ? parsed.Values[0]
                : null,

            ServerBaseUrl = result.TryGetFirstValidOptionParseds(serverUrlOption, out parsed)
                ? parsed.Values[0]
                : null,
            ServerUsername = result.TryGetFirstValidOptionParseds(serverUsernameOption, out parsed)
                ? parsed.Values[0]
                : null,
            ServerPassword = result.TryGetFirstValidOptionParseds(serverPasswordOption, out parsed)
                ? parsed.Values[0]
                : null,

            WithSubfolders = result.TryGetFirstValidOptionParseds(withSubfoldersOption, out parsed)
                ? parsed.Values.Count == 0 || bool.Parse(parsed.Values[0])
                : null,
            LocalFolderPath = result.TryGetFirstValidOptionParseds(localFolderPathOption, out parsed)
                ? parsed.Values[0]
                : null,
            ServerFolderPath = result.TryGetFirstValidOptionParseds(serverFolderPathOption, out parsed)
                ? parsed.Values[0]
                : null,
            Mode = result.TryGetFirstValidOptionParseds(modeOption, out parsed)
                ? (SyncMode)Enum.Parse(typeof(SyncMode), parsed.Values[0])
                : null,
            CompareType = result.TryGetFirstValidOptionParseds(modeOption, out parsed)
                ? (SyncCompareType)Enum.Parse(typeof(SyncCompareType), parsed.Values[0])
                : null,
            ConflictHandling = result.TryGetFirstValidOptionParseds(modeOption, out parsed)
                ? (SyncConflictHandlingType)Enum.Parse(typeof(SyncConflictHandlingType), parsed.Values[0])
                : null,
            AllowList = result.TryGetFirstValidOptionParseds(allowListOption, out parsed)
                ? parsed.Values.ToArray()
                : null,
            DenyList = result.TryGetFirstValidOptionParseds(denyListsOption, out parsed)
                ? parsed.Values.ToArray()
                : null,
            StateFilePath = result.TryGetFirstValidOptionParseds(stateFilePathOption, out parsed)
                ? parsed.Values[0]
                : null,
        };
    }
}
using System.Net;
using FileSystemCLI.Models;
using FileSystemCLI.Services.CompareType;
using FileSystemCLI.Services.Mode;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Sync.Definitions;
using StdOttStandard.Linq;

namespace FileSystemCLI.Services;

public class SyncPairHandler
{
    private const int partialHashSize = 10 * 1024; // 10 kB

    private readonly bool isTestRun;

    private readonly Queue<FilePairModel>
        bothFiles = new(),
        singleFiles = new(),
        copyToLocalFiles = new(),
        copyToServerFiles = new(),
        deleteLocalFiles = new(),
        deleteServerFiles = new();

    private int totalFilesCount, equalFilesCount, ignoreFilesCount, errorFilesCount;

    private readonly HashSet<string> serverFolderExistsCache = new();

    private readonly SyncModeHandler modeHandler;
    private readonly BaseSyncFileComparer fileComparer;
    private readonly SyncPairModel syncPair;
    private readonly Api api;

    public SyncPairState CurrentState { get; } = new SyncPairState();

    public bool IsCancelled { get; private set; }

    public SyncPairHandler(bool isTestRun, SyncPairModel syncPair, Api api, SyncPairState lastState)
    {
        this.syncPair = syncPair;
        this.api = api;
        this.isTestRun = isTestRun;

        fileComparer = GetFileComparer(syncPair.CompareType, api);
        modeHandler = GetSyncModeHandler(syncPair.Mode, fileComparer, lastState, syncPair.ConflictHandling);
    }

    private static SyncModeHandler GetSyncModeHandler(SyncMode mode, BaseSyncFileComparer fileComparer,
        SyncPairState lastState, SyncConflictHandlingType conflictHandlingType)
    {
        switch (mode)
        {
            case SyncMode.ServerToLocalCreateOnly:
                return new ServerToLocalCreateOnlyModeHandler(fileComparer, conflictHandlingType);

            case SyncMode.ServerToLocal:
                return new ServerToLocalModeHandler(fileComparer, conflictHandlingType);

            case SyncMode.LocalToServerCreateOnly:
                return new LocalToServerCreateOnlyModeHandler(fileComparer, conflictHandlingType);

            case SyncMode.LocalToServer:
                return new LocalToServerModeHandler(fileComparer, conflictHandlingType);

            case SyncMode.TwoWay:
                return new TwoWayModeHandler(fileComparer, lastState, conflictHandlingType);
        }

        throw new ArgumentException("Value not Implemented: " + mode, nameof(mode));
    }

    private static BaseSyncFileComparer GetFileComparer(SyncCompareType type, Api api)
    {
        switch (type)
        {
            case SyncCompareType.Exists:
                return new ExistsComparer(api);

            case SyncCompareType.Size:
                return new SizeComparer(api);

            case SyncCompareType.Hash:
                return new HashComparer(api);

            case SyncCompareType.PartialHash:
                return new HashComparer(api, partialHashSize);
        }

        throw new ArgumentException("Value not Implemented:" + type, nameof(type));
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public async Task Run()
    {
        Console.WriteLine("SyncPairHandler is running");

        await QueryFoldersFiles();
        if (IsCancelled) return;
        await CompareSingleFiles();
        if (IsCancelled) return;
        await CompareBothFiles();
        if (IsCancelled) return;
        await CopyFilesToLocal();
        if (IsCancelled) return;
        await CopyFilesToServer();
        if (IsCancelled) return;
        DeleteLocalFiles();
        if (IsCancelled) return;
        await DeleteServerFiles();
        if (IsCancelled) return;


        Console.WriteLine("SyncPairHandler is finished");
        Console.WriteLine($"Total files: {totalFilesCount}");
        Console.WriteLine($"Equal files: {equalFilesCount}");
        Console.WriteLine($"Ignore files: {ignoreFilesCount}");
        Console.WriteLine($"Ignore files: {errorFilesCount}");
        Console.WriteLine($"Both files: {bothFiles.Count}");
        Console.WriteLine($"Single files: {singleFiles.Count}");
        Console.WriteLine($"Copy to local files: {copyToLocalFiles.Count}");
        Console.WriteLine($"Copy to server files: {copyToServerFiles.Count}");
        Console.WriteLine($"Delete local files: {deleteLocalFiles.Count}");
        Console.WriteLine($"Delete server files: {deleteServerFiles.Count}");

        if (isTestRun) Console.WriteLine("THIS WAS A TEST RUN!!");
    }

    public async Task Run(IEnumerable<FilePairModel> filePairs)
    {
        Console.WriteLine("SyncPairHandler is running specific files");

        await QuerySpecificFiles(filePairs);
        if (IsCancelled) return;
        await CompareSingleFiles();
        if (IsCancelled) return;
        await CompareBothFiles();
        if (IsCancelled) return;
        await CopyFilesToLocal();
        if (IsCancelled) return;
        await CopyFilesToServer();
        if (IsCancelled) return;
        DeleteLocalFiles();
        if (IsCancelled) return;
        await DeleteServerFiles();
        if (IsCancelled) return;


        Console.WriteLine("SyncPairHandler is finished specific files");
        Console.WriteLine($"Total files: {totalFilesCount}");
        Console.WriteLine($"Equal files: {equalFilesCount}");
        Console.WriteLine($"Ignore files: {ignoreFilesCount}");
        Console.WriteLine($"Ignore files: {errorFilesCount}");
        Console.WriteLine($"Both files: {bothFiles.Count}");
        Console.WriteLine($"Single files: {singleFiles.Count}");
        Console.WriteLine($"Copy to local files: {copyToLocalFiles.Count}");
        Console.WriteLine($"Copy to server files: {copyToServerFiles.Count}");
        Console.WriteLine($"Delete local files: {deleteLocalFiles.Count}");
        Console.WriteLine($"Delete server files: {deleteServerFiles.Count}");

        if (isTestRun) Console.WriteLine("THIS WAS A TEST RUN!!");
    }

    /// <summary>
    /// Check if file has to be synced based on whitelist and blacklist
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Returns true if file has to be synced</returns>
    private bool CheckAllowAndDenyList(string path)
    {
        if (syncPair.DenyList is not null && syncPair.DenyList.Any(e => path.EndsWith(e))) return false;

        return syncPair.AllowList is null || syncPair.AllowList.Any(e => path.EndsWith(e));
    }

    private FilePairModel CreateFilePair(string relPath, string localFilePath, bool localFileExits,
        string serverFullPath,
        bool serverFileExists)
    {
        string relativePath = relPath.Trim(api.Config.DirectorySeparatorChar);
        return new FilePairModel()
        {
            RelativePath = relativePath,
            ServerFullPath = serverFullPath,
            ServerFileExists = serverFileExists,
            LocalFilePath = localFilePath,
            LocalFileExists = localFileExits,
        };
    }

    private async Task QueryFoldersFiles()
    {
        bool localRootFolderExists = Directory.Exists(syncPair.LocalFolderPath);
        bool serverRootFolderExists = await api.FolderExists(syncPair.ServerFolderPath);

        await Query(string.Empty, syncPair.LocalFolderPath, localRootFolderExists, syncPair.ServerFolderPath,
            serverRootFolderExists);

        async Task Query(string relPath, string localFolderPath, bool localFolderExists, string serverFolderPath,
            bool serverFolderExists)
        {
            if (IsCancelled) return;

            Console.WriteLine($"Query folder: ./{relPath}");

            Task<FolderContent>? serverFolderContentTask =
                serverFolderExists ? api.FolderContent(serverFolderPath) : null;

            IDictionary<string, string> localFiles = localFolderExists
                ? Directory.EnumerateFiles(localFolderPath).ToDictionary(f => Path.GetFileName(f)!)
                : new Dictionary<string, string>();

            FolderContent? serverFolderContent = serverFolderContentTask is null ? null : await serverFolderContentTask;

            if (IsCancelled) return;

            foreach (FileSortItem serverFile in (serverFolderContent?.Files).ToNotNull())
            {
                if (!CheckAllowAndDenyList(serverFile.Path)) continue;

                string relFilePath = Path.Combine(relPath, serverFile.Name);

                if (localFiles.Remove(serverFile.Name, out string? localFilePath))
                {
                    bothFiles.Enqueue(CreateFilePair(relFilePath, localFilePath, true, serverFile.Path, true));
                }
                else
                {
                    localFilePath = Path.Combine(localFolderPath, serverFile.Name);
                    singleFiles.Enqueue(CreateFilePair(relFilePath, localFilePath, false, serverFile.Path, true));
                }

                totalFilesCount++;
            }

            foreach (KeyValuePair<string, string> localFile in localFiles)
            {
                string relFilePath = Path.Combine(relPath, localFile.Key);
                string serverFilePath = api.Config.JoinPaths(serverFolderPath, localFile.Key);

                if (!CheckAllowAndDenyList(serverFilePath)) continue;

                singleFiles.Enqueue(CreateFilePair(relFilePath, localFile.Value, true, serverFilePath, false));
                totalFilesCount++;
            }

            if (!syncPair.WithSubfolders) return;

            IDictionary<string, string> localSubFolders = localFolderExists
                ? Directory.EnumerateDirectories(localFolderPath).ToDictionary(f => Path.GetFileName(f)!)
                : new Dictionary<string, string>();

            foreach (FolderSortItem serverSubFolder in (serverFolderContent?.Folders).ToNotNull())
            {
                string relSubFolderPath = Path.Combine(relPath, serverSubFolder.Name);
                if (localSubFolders.Remove(serverSubFolder.Name, out string? localSubFolderPath))
                {
                    await Query(relSubFolderPath, localSubFolderPath, true, serverSubFolder.Path, true);
                }
                else
                {
                    localSubFolderPath = Path.Combine(localFolderPath, serverSubFolder.Name);
                    await Query(relSubFolderPath, localSubFolderPath, false, serverSubFolder.Path, true);
                }
            }

            foreach (KeyValuePair<string, string> localSubFolder in localSubFolders)
            {
                string relSubFolderPath = Path.Combine(relPath, localSubFolder.Key);
                string localSubFolderPath = Path.Combine(localFolderPath!, localSubFolder.Key);
                string serverSubFolderPath = api.Config.JoinPaths(serverFolderPath, localSubFolder.Key);

                await Query(relSubFolderPath, localSubFolderPath, true, serverSubFolderPath, false);
            }
        }
    }

    private async Task QuerySpecificFiles(IEnumerable<FilePairModel> filePairs)
    {
        Dictionary<string, FilePairModel> dict = filePairs.ToDictionary(p => p.ServerFullPath);

        await api.GetFilesExits(dict.Keys.ToArray(), item =>
        {
            if (IsCancelled || !dict.TryGetValue(item.FilePath, out FilePairModel? pair)) return Task.CompletedTask;

            try
            {
                if (item.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound)
                {
                    pair.ServerFileExists = item.Exists ?? false;
                }
                else
                {
                    Console.WriteLine(
                        $"Error file exists {pair.RelativePath}: HTTP {item.StatusCode} {item.ErrorMessage} ({item.ErrorCode})");
                    return Task.CompletedTask;
                }

                pair.LocalFileExists = File.Exists(pair.LocalFilePath);

                if (!pair.LocalFileExists && !pair.ServerFileExists)
                {
                    Console.WriteLine($"File doesn't exists: {pair.RelativePath}");
                }
                else if (!pair.LocalFileExists || !pair.ServerFileExists) singleFiles.Enqueue(pair);
                else bothFiles.Enqueue(pair);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Compare unknown files error {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
            finally
            {
                dict.Remove(item.FilePath);
            }

            return Task.CompletedTask;
        });

        if (IsCancelled) return;

        foreach (FilePairModel pair in dict.Values)
        {
            Console.WriteLine($"Compare unknown files no response: {pair.RelativePath}");
        }
    }

    private async Task CompareSingleFiles()
    {
        IEnumerable<FilePairModel> singleComparePairs;
        if (modeHandler.PreloadServerCompareValue)
        {
            FilePairModel[] batchComparePairs = singleFiles.Where(f => f.ServerFileExists).ToArray();
            singleComparePairs = singleFiles.Where(f => !f.ServerFileExists);

            await CompareFilesBatch(batchComparePairs, modeHandler.GetActionOfSingleFiles);
        }
        else singleComparePairs = singleFiles;

        foreach (FilePairModel pair in singleComparePairs)
        {
            if (IsCancelled) return;

            try
            {
                SyncActionType action = await modeHandler.GetActionOfSingleFiles(pair);
                if (IsCancelled) return;
                HandleAction(action, pair);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Compare single file error {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
        }
    }

    private async Task CompareBothFiles()
    {
        await CompareFilesBatch(bothFiles.ToArray(), modeHandler.GetActionOfBothFiles);
    }

    private async Task CompareFilesBatch(ICollection<FilePairModel> pairs,
        Func<FilePairModel, Task<SyncActionType>> getActionFunc)
    {
        if (pairs.Count == 0) return;

        Dictionary<string, FilePairModel> dict = pairs.ToDictionary(p => p.ServerFullPath);

        await fileComparer.GetServerCompareValues(dict.Keys.ToArray(), async (path, value, errorMessage) =>
        {
            if (IsCancelled || !dict.TryGetValue(path, out FilePairModel? pair)) return;

            if (value == null)
            {
                Console.WriteLine($"Compare files batch no value: {pair.RelativePath}");
                return;
            }

            try
            {
                pair.ServerCompareValue = value;
                SyncActionType action = await getActionFunc(pair);
                HandleAction(action, pair);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Compare files batch error {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
            finally
            {
                dict.Remove(path);
            }
        });

        if (IsCancelled) return;

        foreach (FilePairModel pair in dict.Values)
        {
            Console.WriteLine($"Compare files batch no response: {pair.RelativePath}");
        }
    }

    private void HandleAction(SyncActionType action, FilePairModel pair)
    {
        switch (action)
        {
            case SyncActionType.CopyToLocal:
                copyToLocalFiles.Enqueue(pair);
                break;

            case SyncActionType.CopyToLocalByConflict:
                copyToLocalFiles.Enqueue(pair);
                break;

            case SyncActionType.CopyToServer:
                copyToServerFiles.Enqueue(pair);
                break;

            case SyncActionType.CopyToServerByConflict:
                copyToServerFiles.Enqueue(pair);
                break;

            case SyncActionType.DeleteFromLocal:
                deleteLocalFiles.Enqueue(pair);
                break;

            case SyncActionType.DeleteFromServer:
                deleteServerFiles.Enqueue(pair);
                break;

            case SyncActionType.Equal:
                Console.WriteLine($"File equal: {pair.RelativePath}");
                CurrentState.AddFile(pair.ToState());
                equalFilesCount++;
                break;

            case SyncActionType.Ignore:
                Console.WriteLine($"File ignore: {pair.RelativePath}");
                ignoreFilesCount++;
                break;
        }
    }

    private async Task CopyFilesToLocal()
    {
        foreach (FilePairModel pair in copyToLocalFiles)
        {
            if (IsCancelled) return;

            string errorMessage = "Unkown";
            string? downloadFilePath = null;
            bool isTmpFile = false;

            try
            {
                if (!isTestRun)
                {
                    errorMessage = "Create local folder error";
                    string localFolderPath = Path.GetDirectoryName(pair.LocalFilePath);
                    Directory.CreateDirectory(localFolderPath);

                    errorMessage = "Create local tmpFile error";
                    (downloadFilePath, isTmpFile) = CheckUseTmpFile(pair.LocalFilePath);


                    errorMessage = "Copy file to local error";
                    await api.DownloadFile(pair.ServerFullPath, downloadFilePath);
                    if (IsCancelled) return;
                    object localCompareValue = await fileComparer.GetLocalCompareValue(downloadFilePath);

                    if (isTmpFile)
                    {
                        File.Move(downloadFilePath, pair.LocalFilePath, true);
                        isTmpFile = false;
                    }

                    pair.LocalCompareValue = localCompareValue;
                    pair.ServerCompareValue ??= await fileComparer.GetServerCompareValue(pair.ServerFullPath);
                    if (IsCancelled) return;

                    CurrentState.AddFile(pair.ToState());
                }

                Console.WriteLine($"Copied file to local: {pair.RelativePath}");
                continue;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error copying file to local {pair.RelativePath}: ({errorMessage}) {e.Message}");
                errorFilesCount++;
            }

            try
            {
                if (isTmpFile && downloadFilePath != null) File.Delete(downloadFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting tmp file {downloadFilePath}: {e.Message}");
            }
        }
    }

    private static (string localWriteFilePath, bool isTmpFile) CheckUseTmpFile(string localFilePath)
    {
        return File.Exists(localFilePath)
            ? ($"{localFilePath}.tmp", true)
            : (localFilePath, false);
    }

    private async Task CopyFilesToServer()
    {
        foreach (FilePairModel pair in copyToServerFiles)
        {
            if (IsCancelled) return;

            try
            {
                if (!isTestRun)
                {
                    if (!await TryCreateServerFolder(pair.ServerFullPath))
                    {
                        Console.WriteLine($"Create server folder, to copy file to, failed: {pair.RelativePath}");
                        errorFilesCount++;
                        continue;
                    }

                    if (IsCancelled) return;

                    await api.UploadFile(pair.ServerFullPath, pair.LocalFilePath);
                    if (IsCancelled) return;

                    pair.LocalCompareValue ??= await fileComparer.GetLocalCompareValue(pair.LocalFilePath);
                    pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath);
                    if (IsCancelled) return;

                    CurrentState.AddFile(pair.ToState());
                }

                Console.WriteLine($"Copied file to server: {pair.RelativePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error copying file to server {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
        }
    }

    private async Task<bool> TryCreateServerFolder(string serverFilePath)
    {
        string parentPath = api.Config.GetParentPath(serverFilePath);
        if (serverFolderExistsCache.Contains(parentPath)) return true;
        if (await api.FolderExists(parentPath))
        {
            serverFolderExistsCache.Add(parentPath);
            return true;
        }

        // act like folder was created successfully to prevent error message
        if (IsCancelled) return true;

        string[] parts = serverFilePath.Split(api.Config.DirectorySeparatorChar);
        string currentFolderPath = string.Empty;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            currentFolderPath = api.Config.JoinPaths(currentFolderPath, parts[i]);

            if (serverFolderExistsCache.Contains(currentFolderPath)) continue;
            if (await api.FolderExists(currentFolderPath))
            {
                serverFolderExistsCache.Add(currentFolderPath);
                continue;
            }

            if (i == 0 || !await api.CreateFolder(currentFolderPath)) return false;

            serverFolderExistsCache.Add(currentFolderPath);
        }

        return true;
    }


    private void DeleteLocalFiles()
    {
        foreach (FilePairModel pair in deleteLocalFiles)
        {
            if (IsCancelled) return;

            try
            {
                if (!isTestRun) File.Delete(pair.LocalFilePath);
                Console.WriteLine($"Deleted file from local: {pair.RelativePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting file from local {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
        }
    }

    private async Task DeleteServerFiles()
    {
        foreach (FilePairModel pair in deleteServerFiles)
        {
            if (IsCancelled) return;

            try
            {
                if (isTestRun || await api.DeleteFile(pair.ServerFullPath))
                {
                    Console.WriteLine($"Deleted file from server: {pair.RelativePath}");
                }
                else
                {
                    Console.WriteLine($"Error deleting file from server: {pair.RelativePath}");
                    errorFilesCount++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting file from server {pair.RelativePath}: {e.Message}");
                errorFilesCount++;
            }
        }
    }
}
using FileSystemCommon.Models.FileSystem.Files.Change;
using FileSystemCommon.Models.FileSystem.Folders.Change;

namespace FileSystemCLI.Services;

public static class ApiExtensions
{
    const long BIG_FILE_SIZE = 30 * 1024 * 1024; // 30 MB
    const long BIG_FILE_CHUNCK_SIZE = 5 * 1024 * 1024; // 5 MB

    public static async Task UploadFile(this Api api, string serverFilePath, string localFilePath)
    {
        FileInfo fileInfo = new FileInfo(localFilePath);
        if (fileInfo.Length <= BIG_FILE_SIZE)
        {
            await using Stream readStream = File.OpenRead(localFilePath);
            if (!await api.UploadFile(serverFilePath, readStream)) throw new Exception("Upload file upload failed");
            return;
        }

        string? uploadUuid = null;
        try
        {
            uploadUuid = await api.StartBigFileUpload(serverFilePath);

            await using Stream readStream = File.OpenRead(localFilePath);

            while (true)
            {
                using MemoryStream memoryStream = new MemoryStream();

                byte[] buffer = new byte[BIG_FILE_CHUNCK_SIZE];
                int bytesRead = await readStream.ReadAsync(buffer, 0, buffer.Length);

                memoryStream.Write(buffer, 0, bytesRead);

                // Jump to start to be readable
                memoryStream.Seek(0, SeekOrigin.Begin);

                System.Diagnostics.Debug.WriteLine($"Upload big file3: {bytesRead}");
                if (bytesRead == 0) break;

                if (!await api.AppendBigFileUpload(uploadUuid, memoryStream))
                    throw new Exception("Append big file upload failed");
            }


            if (!await api.FinishBigFileUpload(uploadUuid)) throw new Exception("Finish big file upload failed");
            uploadUuid = null;
        }
        finally
        {
            if (uploadUuid != null) api.CancelBigFileUpload(uploadUuid);
        }
    }

    public static async Task<List<FileChangeInfo>> GetAllFileChanges(this Api api, string serverFolderPath,
        DateTime since)
    {
        int page = 0;
        int pageSize = 1;
        List<FileChangeInfo> changes = new List<FileChangeInfo>();

        while (true)
        {
            FileChangeResult result = await api.GetFileChanges(serverFolderPath, since, page, pageSize);
            changes.AddRange(result.Changes);

            if (!result.HasMore) break;
            page++;
        }

        return changes;
    }

    public static async Task<List<FolderChangeInfo>> GetAllFolderChanges(this Api api, string serverFolderPath,
        DateTime since)
    {
        int page = 0;
        int pageSize = 1000;
        List<FolderChangeInfo> changes = new List<FolderChangeInfo>();

        while (true)
        {
            FolderChangeResult result = await api.GetFolderChanges(serverFolderPath, since, page, pageSize);
            changes.AddRange(result.Changes);

            if (!result.HasMore) break;
            page++;
        }

        return changes;
    }
}
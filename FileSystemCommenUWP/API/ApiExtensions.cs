using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace FileSystemCommonUWP.API
{
    public static class ApiExtensions
    {
        const ulong BIG_FILE_SIZE = 30 * 1024 * 1024; // 30 MB
        const ulong BIG_FILE_CHUNCK_SIZE = 5 * 1024 * 1024; // 5 MB

        public static async Task<bool> UploadFile(this Api api, string path, StorageFile file)
        {
            BasicProperties properties = await file.GetBasicPropertiesAsync();
            if (properties.Size <= BIG_FILE_SIZE)
            {
                using (IInputStream readStream = await file.OpenReadAsync())
                {
                    return await api.UploadFile(path, readStream);
                }
            }

            string uploadUuid = null;
            try
            {
                uploadUuid = await api.StartBigFileUpload(path);

                using (IInputStream readStream = await file.OpenReadAsync())
                {
                    byte[] buffer = new byte[BIG_FILE_CHUNCK_SIZE];
                    while (true)
                    {
                        IBuffer result = await readStream.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.ReadAhead);
                        System.Diagnostics.Debug.WriteLine($"Upload big file3: {result.Length}");
                        if (result.Length == 0) break;

                        if (!await api.AppendBigFileUpload(uploadUuid, result)) return false;
                    }
                }

                if (!await api.FinishBigFileUpload(uploadUuid)) return false;
                uploadUuid = null;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (uploadUuid != null) api.CancelBigFileUpload(uploadUuid);
            }
        }
    }
}

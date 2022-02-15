using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileSystemUWP.API
{
    static class ApiExtensions
    {
        public static async Task<bool> UploadFile(this Api api, string path, StorageFile file)
        {
            using (IInputStream readStream = await file.OpenReadAsync())
            {
                return await api.UploadFile(path, readStream);
            }
        }
    }
}

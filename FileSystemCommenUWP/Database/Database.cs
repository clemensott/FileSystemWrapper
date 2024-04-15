using FileSystemCommonUWP.Database.Servers;
using FileSystemCommonUWP.Database.SyncPairs;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Database
{
    public class AppDatabase : IDisposable
    {
        public ServersRepo Servers { get; }

        public SyncPairsRepos SyncPairs { get; }

        public AppDatabase(ISqlExecuteService sqlExecuteService)
        {
            Servers = new ServersRepo(sqlExecuteService);
            SyncPairs = new SyncPairsRepos(sqlExecuteService);
        }

        public async Task Init()
        {
            await Servers.Init();
            await SyncPairs.Init();
        }

        public static async Task<AppDatabase> OpenSqlite()
        {
            StorageFile dbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("servers.db", CreationCollisionOption.OpenIfExists);
            return FromSqlite(dbFile);
        }

        public static AppDatabase FromSqlite(StorageFile file)
        {
            string connectionString = $"Filename={file.Path}";
            return new AppDatabase(new SqliteExecuteService(connectionString));
        }

        public void Dispose()
        {
            Servers?.Dispose();
            SyncPairs?.Dispose();
        }
    }
}

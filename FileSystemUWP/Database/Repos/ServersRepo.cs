using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommonUWP.API;
using FileSystemUWP.Controls;
using FileSystemUWP.Picker;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemUWP.Database.Repos
{
    class ServersRepo : BaseRepo
    {
        public ServersRepo(ISqlExecuteService sqlExecuteService) : base(sqlExecuteService)
        {
        }

        private static Api CreateApiObject(DbDataReader reader)
        {
            return new Api()
            {
                Name = reader.GetString("name"),
                BaseUrl = reader.GetString("base_url"),
                Username = reader.GetString("username"),
                RawCookies = JsonConvert.DeserializeObject<string[]>(reader.GetString("raw_cookies")),
            };
        }

        private static FileSystemItemSortBy CreateSortByObject(DbDataReader reader)
        {
            return new FileSystemItemSortBy()
            {
                Type = (FileSystemItemSortType)reader.GetInt64("sort_by_type"),
                Direction = (FileSystemItemSortDirection)reader.GetInt64("sort_by_direction"),
            };
        }

        private static FileSystemSortItem? CreateRestoreFileSystemItemObject(DbDataReader reader)
        {
            long? isFile = reader.GetInt64("restore_item_is_file");
            string name = reader.GetString("restore_item_name");
            string[] sortKeys = JsonConvert.DeserializeObject<string[]>(reader.GetString("restore_item_name"));

            if (isFile.HasValue && name != null)
            {
                return new FileSystemSortItem(isFile.Value == 1L, name, sortKeys?.ToList().AsReadOnly());
            }

            return null;
        }

        private static Server CreateServerObject(DbDataReader reader)
        {
            return new Server()
            {
                Id = (int)reader.GetInt64("id"),
                Api = CreateApiObject(reader),
                SortBy = CreateSortByObject(reader),
                CurrentFolderPath = reader.GetString("current_folder_path"),
                RestoreFileSystemItem = CreateRestoreFileSystemItemObject(reader),
            };
        }

        public async Task<IEnumerable<Server>> SelectServers(BackgroundOperations backgroundOperations)
        {
            const string sql = @"
                SELECT id, name, base_url, username, raw_cookies, current_folder_path, sort_by_type, sort_by_direction, 
                    restore_item_is_file, restore_item_name, restore_item_sort_keys
                FROM servers;
            ";

            return await sqlExecuteService.ExecuteReadAllAsync(CreateServerObject, sql);
        }

        private static long? ToNullableLong(bool? value)
        {
            if (value.HasValue)
            {
                return value.Value ? 1L : 0L;
            }

            return null;
        }

        public async Task ÎnsertServer(Server server)
        {
            const string sql = @"
                INSERT INTO servers (name, base_url, username, raw_cookies, current_folder_path, sort_by_type, sort_by_direction, 
                    restore_item_is_file, restore_item_name, restore_item_sort_keys)
                VALUES (@name, @baseUrl, @username, @rawCookies, @currentFolderPath, @sortByType, @sortByDirection,
                    @restoreItemIsFile, @restoreItemName, @restoreItemSortKeys);
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("name", server.Api.Name),
                new KeyValuePair<string, object>("baseUrl", server.Api.BaseUrl),
                new KeyValuePair<string, object>("username", server.Api.Username),
                new KeyValuePair<string, object>("rawCookies", JsonConvert.SerializeObject(server.Api.RawCookies)),
                new KeyValuePair<string, object>("currentFolderPath", server.CurrentFolderPath),
                new KeyValuePair<string, object>("sortByType", (long)server.SortBy.Type),
                new KeyValuePair<string, object>("sortByDirection", (long)server.SortBy.Direction),
                new KeyValuePair<string, object>("restoreItemIsFile", ToNullableLong(server.RestoreFileSystemItem?.IsFile)),
                new KeyValuePair<string, object>("restoreItemName", server.RestoreFileSystemItem?.Name),
                new KeyValuePair<string, object>("restoreItemSortKeys", JsonConvert.SerializeObject(server.RestoreFileSystemItem?.SortKeys?.ToArray())),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
            long serverId = await sqlExecuteService.ExecuteScalarAsync<long>("SELECT last_insert_id()");
            server.Id = (int)serverId;
        }

        public async Task UpdateServer(Server server)
        {
            const string sql = @"
                UPDATE servers
                SET name = @name,
                    base_url = @baseUrl,
                    username = @username,
                    raw_cookies = @rawCookies,
                    current_folder_path = @currentFolderPath,
                    sort_by_type = @sortByType,
                    sort_by_direction = @sortByDirection,
                    restore_item_is_file = @restoreItemIsFile,
                    restore_item_name = @restoreItemName,
                    restore_item_sort_keys = @restoreItemSortKeys
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("id", (long)server.Id),
                new KeyValuePair<string, object>("name", server.Api.Name),
                new KeyValuePair<string, object>("baseUrl", server.Api.BaseUrl),
                new KeyValuePair<string, object>("username", server.Api.Username),
                new KeyValuePair<string, object>("rawCookies", JsonConvert.SerializeObject(server.Api.RawCookies)),
                new KeyValuePair<string, object>("currentFolderPath", server.CurrentFolderPath),
                new KeyValuePair<string, object>("sortByType", (int)server.SortBy.Type),
                new KeyValuePair<string, object>("sortByDirection", (int)server.SortBy.Direction),
                new KeyValuePair<string, object>("restoreItemIsFile", ToNullableLong(server.RestoreFileSystemItem?.IsFile)),
                new KeyValuePair<string, object>("restoreItemName", server.RestoreFileSystemItem?.Name),
                new KeyValuePair<string, object>("restoreItemSortKeys", JsonConvert.SerializeObject(server.RestoreFileSystemItem?.SortKeys?.ToArray())),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteServer(Server server)
        {
            const string sql = @"
                DELETE FROM sync_pairs WHERE server_id = @serverId;
                DELETE FROM servers WHERE id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("serverId", server.Id),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}

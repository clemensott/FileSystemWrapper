using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommonUWP.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Database.Servers
{
    public class ServersRepo : BaseRepo
    {
        public ServersRepo(ISqlExecuteService sqlExecuteService) : base(sqlExecuteService)
        {
        }

        public async Task Init()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS servers (
                    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    is_current              INTEGER NOT NULL,
                    name                    TEXT NOT NULL,
                    base_url                TEXT NOT NULL,
                    username                TEXT NOT NULL,
                    raw_cookies             TEXT NOT NULL,
                    current_folder_path     TEXT,
                    sort_by_type            INTEGER NOT NULL,
                    sort_by_direction       INTEGER NOT NULL,
                    restore_item_is_file    INTEGER,
                    restore_item_name       TEXT,
                    restore_item_sort_keys  TEXT,
                    created                 TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
            ";

            await sqlExecuteService.ExecuteNonQueryAsync(sql);
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

        private static ServerInfo CreateServerObject(DbDataReader reader)
        {
            return new ServerInfo()
            {
                Id = (int)reader.GetInt64("id"),
                Api = CreateApiObject(reader),
                SortBy = CreateSortByObject(reader),
                CurrentFolderPath = reader.GetString("current_folder_path"),
                RestoreIsFile = reader.GetBooleanFromNullableLong("restore_item_is_file"),
                RestoreName = reader.GetString("restore_item_is_file"),
                RestoreSortKeys = JsonConvert.DeserializeObject<string[]>(reader.GetString("restore_sort_keys")),
            };
        }

        public async Task<IList<ServerInfo>> SelectServers()
        {
            const string sql = @"
                SELECT id, name, base_url, username, raw_cookies, current_folder_path, sort_by_type, sort_by_direction, 
                    restore_item_is_file, restore_item_name, restore_item_sort_keys
                FROM servers;
            ";

            return await sqlExecuteService.ExecuteReadAllAsync(CreateServerObject, sql);
        }

        public async Task InsertServer(ServerInfo server)
        {
            const string sql = @"
                INSERT INTO servers (name, base_url, username, raw_cookies, current_folder_path, sort_by_type, sort_by_direction, 
                    restore_item_is_file, restore_item_name, restore_item_sort_keys)
                VALUES (@name, @baseUrl, @username, @rawCookies, @currentFolderPath, @sortByType, @sortByDirection,
                    @restoreItemIsFile, @restoreItemName, @restoreItemSortKeys);

                SELECT last_insert_id();
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("name", server.Api.Name),
                CreateParam("baseUrl", server.Api.BaseUrl),
                CreateParam("username", server.Api.Username),
                CreateParam("rawCookies", JsonConvert.SerializeObject(server.Api.RawCookies)),
                CreateParam("currentFolderPath", server.CurrentFolderPath),
                CreateParam("sortByType", (long)server.SortBy.Type),
                CreateParam("sortByDirection", (long)server.SortBy.Direction),
                CreateParam("restoreItemIsFile", ToNullableLong(server.RestoreIsFile)),
                CreateParam("restoreItemName", server.RestoreName),
                CreateParam("restoreItemSortKeys", JsonConvert.SerializeObject(server.RestoreSortKeys?.ToArray())),
            };

            long serverId = await sqlExecuteService.ExecuteScalarAsync<long>(sql, parameters);
            server.Id = (int)serverId;
        }

        public async Task UpdateServer(ServerInfo server)
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
                CreateParam("id", (long)server.Id),
                CreateParam("name", server.Api.Name),
                CreateParam("baseUrl", server.Api.BaseUrl),
                CreateParam("username", server.Api.Username),
                CreateParam("rawCookies", JsonConvert.SerializeObject(server.Api.RawCookies)),
                CreateParam("currentFolderPath", server.CurrentFolderPath),
                CreateParam("sortByType", (int)server.SortBy.Type),
                CreateParam("sortByDirection", (int)server.SortBy.Direction),
                CreateParam("restoreItemIsFile", ToNullableLong(server.RestoreIsFile)),
                CreateParam("restoreItemName", server.RestoreName),
                CreateParam("restoreItemSortKeys", JsonConvert.SerializeObject(server.RestoreSortKeys?.ToArray())),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteServer(ServerInfo server)
        {
            const string sql = @"
                DELETE FROM sync_pairs WHERE server_id = @serverId;
                -- TODO: delete other tables
                DELETE FROM servers WHERE id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", server.Id),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int?> SelectCurrentServerId()
        {
            const string sql = @"
                SELECT id
                FROM servers
                WHERE is_current
                LIMIT 1;
            ";

            object currentServerId = await sqlExecuteService.ExecuteScalarAsync(sql);
            return currentServerId is DBNull ? (int?)null : (int)(long)currentServerId;
        }

        public async Task UpdateCurrentServer(int? serverId)
        {
            const string sql = @"
                UPDATE servers
                SET is_current = false
                WHERE is_current;

                UPDATE servers
                SET is_current = true
                WHERE id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", serverId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}

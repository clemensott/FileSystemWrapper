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
                    is_current              INTEGER NOT NULL DEFAULT false,
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
            string restoreSortKeys = reader.GetStringNullable("restore_item_sort_keys");
            return new ServerInfo()
            {
                Id = (int)reader.GetInt64("id"),
                Api = CreateApiObject(reader),
                SortBy = CreateSortByObject(reader),
                CurrentFolderPath = reader.GetStringNullable("current_folder_path"),
                RestoreIsFile = reader.GetBooleanFromNullableLong("restore_item_is_file"),
                RestoreName = reader.GetStringNullable("restore_item_name"),
                RestoreSortKeys = restoreSortKeys == null ? null : JsonConvert.DeserializeObject<string[]>(restoreSortKeys),
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

                SELECT last_insert_rowid();
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

        private async Task<IList<int>> SelectSyncPairValueIds(int serverId, string columnName)
        {
            string sql = $@"
                SELECT {columnName} as value_id
                FROM sync_pairs
                WHERE server_id = @serverId
                    AND {columnName} IS NOT NULL
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", serverId),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(reader => (int)reader.GetInt64("value_id"), sql, parameters);
        }

        private async Task<IList<int>> SelecCurrentSyncPairRunIds(int serverId)
        {
            return await SelectSyncPairValueIds(serverId, "current_sync_pair_run_id");
        }

        private async Task<IList<int>> SelectLastSyncPairResultIds(int serverId)
        {
            return await SelectSyncPairValueIds(serverId, "last_sync_pair_result_id");
        }

        public async Task DeleteServer(ServerInfo server)
        {
            IList<int> syncPairRunIdsToDelete = await SelecCurrentSyncPairRunIds(server.Id);
            IList<int> syncPairResultIdsToDelete = await SelectLastSyncPairResultIds(server.Id);

            string syncPairRunIdsValues = string.Concat(syncPairRunIdsToDelete.Select((_, i) => $"@runId{i},"));
            string syncPairResultIdsValues = string.Concat(syncPairResultIdsToDelete.Select((_, i) => $"@resultId{i},"));

            string sql = $@"
                DELETE FROM sync_pairs WHERE server_id = @serverId;
                DELETE FROM sync_pair_result_files WHERE sync_pair_result_id IN ({syncPairResultIdsValues}0);
                DELETE FROM sync_pair_results WHERE id IN ({syncPairResultIdsValues}0);
                DELETE FROM sync_pair_run_files WHERE sync_pair_run_id IN ({syncPairRunIdsValues}0);
                DELETE FROM sync_pair_runs WHERE id IN ({syncPairRunIdsValues}0);
                DELETE FROM servers WHERE id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", server.Id),
            }
            .Concat(syncPairRunIdsToDelete.Select((id, i) => CreateParam($"runId{i}", (long)id)))
            .Concat(syncPairResultIdsToDelete.Select((id, i) => CreateParam($"resultId{i}", (long)id)));

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
            return currentServerId is null || currentServerId is DBNull ? (int?)null : (int)(long)currentServerId;
        }

        public async Task UpdateCurrentServer(int? serverId)
        {
            const string sql = @"
                UPDATE servers
                SET is_current = 0
                WHERE is_current;

                UPDATE servers
                SET is_current = 1
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

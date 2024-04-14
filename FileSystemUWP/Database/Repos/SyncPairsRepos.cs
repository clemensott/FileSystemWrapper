using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.Communication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Threading.Tasks;

namespace FileSystemUWP.Database.Repos
{
    class SyncPairsRepos : BaseRepo
    {
        public SyncPairsRepos(ISqlExecuteService sqlExecuteService) : base(sqlExecuteService)
        {
        }

        private static SyncPair CreateSyncPairObject(DbDataReader reader)
        {
            string[] allowList = JsonConvert.DeserializeObject<string[]>(reader.GetString("allow_list"));
            string[] denyList = JsonConvert.DeserializeObject<string[]>(reader.GetString("deny_list"));

            return new SyncPair()
            {
                Id = (int)reader.GetInt64("id"),
                WithSubfolders = reader.GetInt64("with_subfolders") == 1L,
                Name = reader.GetString("name"),
                Mode = (SyncMode)reader.GetInt64("mode"),
                CompareType = (SyncCompareType)reader.GetInt64("compare_type"),
                ConflictHandlingType = (SyncConflictHandlingType)reader.GetInt64("conflict_handling_type"),
                AllowList = allowList == null ? null : new ObservableCollection<string>(allowList),
                DenyList = allowList == null ? null : new ObservableCollection<string>(denyList),
            };
        }

        public async Task<IEnumerable<SyncPair>> SelectSyncPairs(int serverId)
        {
            const string sql = @"
                SELECT id, name, server_path, with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list
                FROM sync_pairs
                WHERE server_id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("id", (long)serverId),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairObject, sql, parameters);
        }

        public async Task InsertSyncPair(SyncPair pair)
        {
            const string sql = @"
                INSERT INTO sync_pairs (name, server_path, with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list)
                VALUES (@name, @serverPath, @withSubfolders, @mode, @compareType, @conflictHandlingType, @allowList, @denyList);
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("name", pair.Name),
                new KeyValuePair<string, object>("serverPath", pair.ServerPath),
                new KeyValuePair<string, object>("withSubfolders", pair.WithSubfolders),
                new KeyValuePair<string, object>("mode", (long)pair.Mode),
                new KeyValuePair<string, object>("compareType", (long)pair.CompareType),
                new KeyValuePair<string, object>("conflictHandlingType", (long)pair.ConflictHandlingType),
                new KeyValuePair<string, object>("allowList", JsonConvert.SerializeObject(pair.AllowList)),
                new KeyValuePair<string, object>("denyList", JsonConvert.SerializeObject(pair.DenyList)),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
            long syncPairId = await sqlExecuteService.ExecuteScalarAsync<long>("SELECT last_insert_id()");
            pair.Id = (int)syncPairId;
        }

        public async Task UpdateSyncPair(SyncPair pair)
        {
            const string sql = @"
                UPDATE sync_pairs
                SET name = @name,
                    server_path = @serverPath,
                    with_subfolders = @withSubfolders,
                    mode = @mode,
                    compare_type = @compareType,
                    conflict_handling_type = @conflictHandlingType,
                    allow_list = @allowList,
                    deny_list = @denyList
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("id", (long)pair.Id),
                new KeyValuePair<string, object>("name", pair.Name),
                new KeyValuePair<string, object>("serverPath", pair.ServerPath),
                new KeyValuePair<string, object>("withSubfolders", pair.WithSubfolders ? 1L : 0L),
                new KeyValuePair<string, object>("mode", (long)pair.Mode),
                new KeyValuePair<string, object>("compareType", (long)pair.CompareType),
                new KeyValuePair<string, object>("conflictHandlingType", (long)pair.ConflictHandlingType),
                new KeyValuePair<string, object>("allowList", JsonConvert.SerializeObject(pair.AllowList)),
                new KeyValuePair<string, object>("denyList", JsonConvert.SerializeObject(pair.DenyList)),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteSyncPair(SyncPair pair)
        {
            const string sql = @"
                -- TODO: delete sync pair in other tables
                DELETE FROM sync_pairs WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("id", pair.Id),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<SyncPairRequestInfo> SelectLatestSyncPairRun(int syncPairId)
        {

        }

        public async Task InsertSyncPairRequest(int syncPairId, SyncPairRequestInfo request)
        {

        }

        public async Task<SyncPairResponseInfo> SelectSyncPairResponse(int syncPairRequestId)
        {

        }

        public async Task
    }
}

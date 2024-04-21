using FileSystemCommon.Models.FileSystem;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using Newtonsoft.Json;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Database.SyncPairs
{
    public class SyncPairsRepos : BaseRepo
    {
        internal SyncPairsRepos(ISqlExecuteService sqlExecuteService) : base(sqlExecuteService)
        {
        }

        public async Task Init()
        {
            const string sql = @"
-- Drop table sync_pair_results;
                CREATE TABLE IF NOT EXISTS sync_pair_results (
                    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    created                 TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

-- Drop table sync_pair_result_files;
                CREATE TABLE IF NOT EXISTS sync_pair_result_files (
                    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    sync_pair_result_id     INTEGER NOT NULL REFERENCES sync_pair_results(id),
                    relative_path           TEXT NOT NULL,
                    local_compare_value     TEXT NOT NULL,
                    server_compare_value    TEXT NOT NULL,
                    created                 TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(sync_pair_result_id, relative_path)
                );

-- Drop table sync_pair_runs;
                CREATE TABLE IF NOT EXISTS sync_pair_runs (
                    id                                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    with_sub_folders                    INTEGER NOT NULL,
                    is_test_run                         INTEGER NOT NULL,
                    requested_cancel                    INTEGER NOT NULL,
                    mode                                INTEGER NOT NULL,
                    compare_type                        INTEGER NOT NULL,
                    conflict_handling_type              INTEGER NOT NULL,
                    name                                TEXT NOT NULL,
                    local_folder_token                  TEXT NOT NULL,
                    local_folder_path                   TEXT,
                    server_name_path                    TEXT NOT NULL,
                    server_path                         TEXT NOT NULL,
                    allow_list                          TEXT NOT NULL,
                    deny_list                           TEXT NOT NULL,
                    api_base_url                        TEXT NOT NULL,
                    state                               INTEGER NOT NULL,
                    current_count                       INTEGER NOT NULL,
                    all_files_count                     INTEGER NOT NULL,
                    compared_files_count                INTEGER NOT NULL,
                    equal_files_count                   INTEGER NOT NULL,
                    conflict_files_count                INTEGER NOT NULL,
                    copied_local_files_count            INTEGER NOT NULL,
                    copied_server_files_count           INTEGER NOT NULL,
                    deleted_local_files_count           INTEGER NOT NULL,
                    deleted_server_files_count          INTEGER NOT NULL,
                    error_files_count                   INTEGER NOT NULL,
                    ignore_files_count                  INTEGER NOT NULL,
                    current_query_folder_rel_path       TEXT,
                    current_copy_to_local_rel_path      TEXT,
                    current_copy_to_server_rel_path     TEXT,
                    current_delete_from_server_rel_path TEXT,
                    current_delete_from_local_rel_path  TEXT,
                    created                             TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

-- Drop table sync_pair_run_files;
                CREATE TABLE IF NOT EXISTS sync_pair_run_files (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    sync_pair_run_id    INTEGER NOT NULL REFERENCES sync_pair_runs(id),
                    relative_path       TEXT NOT NULL,
                    type                INTEGER NOT NULL,
                    name                TEXT NOT NULL,
                    error_message       TEXT,
                    error_stackstrace   TEXT,
                    error_exception     TEXT,
                    created             TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(sync_pair_run_id, relative_path)
                );

-- Drop table sync_pairs;
                CREATE TABLE IF NOT EXISTS sync_pairs (
                    id                          INTEGER PRIMARY KEY AUTOINCREMENT,
                    server_id                   INTEGER NOT NULL REFERENCES servers(id),
                    current_sync_pair_run_id    INTEGER REFERENCES sync_pair_runs(id),
                    last_sync_pair_result_id    INTEGER REFERENCES sync_pair_results(id),
                    local_folder_token          TEXT NOT NULL,
                    name                        TEXT NOT NULL,
                    server_path                 TEXT NOT NULL,
                    with_subfolders             INTEGER NOT NULL,
                    mode                        INTEGER NOT NULL,
                    compare_type                INTEGER NOT NULL,
                    conflict_handling_type      INTEGER NOT NULL,
                    allow_list                  TEXT NOT NULL,
                    deny_list                   TEXT NOT NULL,
                    created                     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(server_id, name)
                );
            ";

            await sqlExecuteService.ExecuteNonQueryAsync(sql);
        }

        private static SyncPair CreateSyncPairObject(DbDataReader reader)
        {
            int serverId = (int)reader.GetInt64("server_id");
            string[] allowList = JsonConvert.DeserializeObject<string[]>(reader.GetString("allow_list"));
            string[] denyList = JsonConvert.DeserializeObject<string[]>(reader.GetString("deny_list"));

            return new SyncPair(serverId, reader.GetString("local_folder_token"))
            {
                Id = (int)reader.GetInt64("id"),
                CurrentSyncPairRunId = (int?)reader.GetInt64Nullable("current_sync_pair_run_id"),
                LastSyncPairResultId = (int?)reader.GetInt64Nullable("last_sync_pair_result_id"),
                WithSubfolders = reader.GetInt64("with_subfolders") == 1L,
                Name = reader.GetString("name"),
                ServerPath = JsonConvert.DeserializeObject<PathPart[]>(reader.GetString("server_path")),
                Mode = (SyncMode)reader.GetInt64("mode"),
                CompareType = (SyncCompareType)reader.GetInt64("compare_type"),
                ConflictHandlingType = (SyncConflictHandlingType)reader.GetInt64("conflict_handling_type"),
                AllowList = new ObservableCollection<string>(allowList.ToNotNull()),
                DenyList = new ObservableCollection<string>(denyList.ToNotNull()),
            };
        }

        public async Task<IList<SyncPair>> SelectSyncPairs(int serverId)
        {
            const string sql = @"
                SELECT id, server_id, current_sync_pair_run_id, last_sync_pair_result_id, local_folder_token, name, server_path,
                    with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list
                FROM sync_pairs
                WHERE server_id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", (long)serverId),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairObject, sql, parameters);
        }

        public async Task InsertSyncPair(SyncPair pair)
        {
            const string sql = @"
                INSERT INTO sync_pairs (server_id, current_sync_pair_run_id, last_sync_pair_result_id, local_folder_token, name, server_path,
                    with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list)
                VALUES (@serverId, @currentSyncPairRunId, @lastSyncPairResultId, @localFolderToken, @name, @serverPath,
                    @withSubfolders, @mode, @compareType, @conflictHandlingType, @allowList, @denyList);

                SELECT last_insert_rowid();
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("serverId", pair.ServerId),
                CreateParam("currentSyncPairRunId", pair.CurrentSyncPairRunId),
                CreateParam("lastSyncPairResultId", pair.LastSyncPairResultId),
                CreateParam("localFolderToken", pair.LocalFolderToken),
                CreateParam("name", pair.Name),
                CreateParam("serverPath", JsonConvert.SerializeObject(pair.ServerPath)),
                CreateParam("withSubfolders", pair.WithSubfolders),
                CreateParam("mode", (long)pair.Mode),
                CreateParam("compareType", (long)pair.CompareType),
                CreateParam("conflictHandlingType", (long)pair.ConflictHandlingType),
                CreateParam("allowList", JsonConvert.SerializeObject(pair.AllowList)),
                CreateParam("denyList", JsonConvert.SerializeObject(pair.DenyList)),
            };

            long syncPairId = await sqlExecuteService.ExecuteScalarAsync<long>(sql, parameters);
            pair.Id = (int)syncPairId;
        }

        public async Task UpdateSyncPair(SyncPair pair)
        {
            const string sql = @"
                UPDATE sync_pairs
                SET server_id = @serverId,
                    current_sync_pair_run_id = @currentSyncPairRunId,
                    last_sync_pair_result_id = @lastSyncPairResultId,
                    name = @name,
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
                CreateParam("id", (long)pair.Id),
                CreateParam("serverId", (long)pair.ServerId),
                CreateParam("currentSyncPairRunId", pair.CurrentSyncPairRunId),
                CreateParam("lastSyncPairResultId", pair.LastSyncPairResultId),
                CreateParam("name", pair.Name),
                CreateParam("serverPath", JsonConvert.SerializeObject(pair.ServerPath)),
                CreateParam("withSubfolders", pair.WithSubfolders ? 1L : 0L),
                CreateParam("mode", (long)pair.Mode),
                CreateParam("compareType", (long)pair.CompareType),
                CreateParam("conflictHandlingType", (long)pair.ConflictHandlingType),
                CreateParam("allowList", JsonConvert.SerializeObject(pair.AllowList)),
                CreateParam("denyList", JsonConvert.SerializeObject(pair.DenyList)),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteSyncPair(SyncPair pair)
        {
            const string sql = @"
                DELETE FROM sync_pairs WHERE id = @syncPairId;
                DELETE FROM sync_pair_run_files WHERE sync_pair_run_id = @currentSyncPairRunId;
                DELETE FROM sync_pair_runs WHERE id = @currentSyncPairRunId;
                DELETE FROM sync_pair_result_files WHERE sync_pair_result_id = @lastSyncPairResultId;
                DELETE FROM sync_pair_results WHERE id = @lastSyncPairResultId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairId", pair.Id),
                CreateParam("currentSyncPairRunId", pair.CurrentSyncPairRunId),
                CreateParam("lastSyncPairResultId", pair.LastSyncPairResultId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        private static SyncPairRun CreateSyncPairRunObject(DbDataReader reader)
        {
            return new SyncPairRun()
            {
                Id = (int)reader.GetInt64("id"),
                WithSubfolders = reader.GetInt64("with_sub_folders") == 1L,
                IsTestRun = reader.GetInt64("is_test_run") == 1L,
                RequestedCancel = reader.GetInt64("requested_cancel") == 1L,
                Mode = (SyncMode)reader.GetInt64("mode"),
                CompareType = (SyncCompareType)reader.GetInt64("compare_type"),
                ConflictHandlingType = (SyncConflictHandlingType)reader.GetInt64("conflict_handling_type"),
                Name = reader.GetString("name"),
                LocalFolderToken = reader.GetString("local_folder_token"),
                ServerNamePath = reader.GetString("server_name_path"),
                ServerPath = reader.GetString("server_path"),
                AllowList = JsonConvert.DeserializeObject<string[]>(reader.GetString("allow_list")),
                DenyList = JsonConvert.DeserializeObject<string[]>(reader.GetString("deny_list")),
                ApiBaseUrl = reader.GetString("api_base_url"),
                State = (SyncPairHandlerState)reader.GetInt64("state"),
                CurrentCount = (int)reader.GetInt64("current_count"),
                AllFilesCount = (int)reader.GetInt64("all_files_count"),
                ComparedFilesCount = (int)reader.GetInt64("compared_files_count"),
                EqualFilesCount = (int)reader.GetInt64("equal_files_count"),
                ConflictFilesCount = (int)reader.GetInt64("conflict_files_count"),
                CopiedLocalFilesCount = (int)reader.GetInt64("copied_local_files_count"),
                CopiedServerFilesCount = (int)reader.GetInt64("copied_server_files_count"),
                DeletedLocalFilesCount = (int)reader.GetInt64("deleted_local_files_count"),
                DeletedServerFilesCount = (int)reader.GetInt64("deleted_server_files_count"),
                ErrorFilesCount = (int)reader.GetInt64("error_files_count"),
                IgnoreFilesCount = (int)reader.GetInt64("ignore_files_count"),
                LocalFolderPath = reader.GetStringNullable("local_folder_path"),
                CurrentQueryFolderRelPath = reader.GetStringNullable("current_query_folder_rel_path"),
                CurrentCopyToLocalRelPath = reader.GetStringNullable("current_copy_to_local_rel_path"),
                CurrentCopyToServerRelPath = reader.GetStringNullable("current_copy_to_server_rel_path"),
                CurrentDeleteFromServerRelPath = reader.GetStringNullable("current_delete_from_server_rel_path"),
                CurrentDeleteFromLocalRelPath = reader.GetStringNullable("current_delete_from_local_rel_path"),
            };
        }

        public async Task<IList<SyncPairRun>> SelectSyncPairRuns(IEnumerable<int> syncPairRunIds)
        {
            string idValues = string.Join(",", syncPairRunIds.Select((_, i) => $"@id{i}"));
            string sql = $@"
                SELECT id, with_sub_folders, is_test_run, requested_cancel, mode, compare_type, conflict_handling_type,
                    name, local_folder_token, server_name_path, server_path, allow_list, deny_list, api_base_url,
                    state, current_count, all_files_count, compared_files_count, equal_files_count, conflict_files_count,
                    copied_local_files_count, copied_server_files_count, deleted_local_files_count, deleted_server_files_count,
                    error_files_count, ignore_files_count, local_folder_path, current_query_folder_rel_path, current_copy_to_local_rel_path,
                    current_copy_to_server_rel_path, current_delete_from_server_rel_path, current_delete_from_local_rel_path
                FROM sync_pair_runs
                WHERE id IN ({idValues});
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = syncPairRunIds.Select((id, i) => CreateParam($"id{i}", (long)id));

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairRunObject, sql, parameters);
        }

        public async Task<int?> SelectNextSyncPairRunId()
        {
            const string sql = @"
                SELECT id
                FROM sync_pair_runs
                WHERE state IN (@loading, @waitForStart, @running)
                LIMIT 1;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("loading", (long)SyncPairHandlerState.Loading),
                CreateParam("waitForStart", (long)SyncPairHandlerState.WaitForStart),
                CreateParam("running", (long)SyncPairHandlerState.Running),
            };

            object nextSyncPairRunId = await sqlExecuteService.ExecuteScalarAsync(sql, parameters);
            return nextSyncPairRunId is null || nextSyncPairRunId is DBNull ? (int?)null : (int)(long)nextSyncPairRunId;
        }

        public async Task InsertSyncPairRun(SyncPair pair, SyncPairRun run)
        {
            const string sql = @"
                INSERT INTO sync_pair_runs (with_sub_folders, is_test_run, requested_cancel, mode, compare_type, conflict_handling_type,
                    name, local_folder_token, server_name_path, server_path, allow_list, deny_list, api_base_url,
                    state, current_count, all_files_count, compared_files_count, equal_files_count, conflict_files_count,
                    copied_local_files_count, copied_server_files_count, deleted_local_files_count, deleted_server_files_count,
                    error_files_count, ignore_files_count, local_folder_path, current_query_folder_rel_path, current_copy_to_local_rel_path,
                    current_copy_to_server_rel_path, current_delete_from_server_rel_path, current_delete_from_local_rel_path)
                VALUES (@withSubfolders, @isTestRun, @requestedCancel, @mode, @compareType, @conflictHandlingType,
                    @name, @localFolderToken, @serverNamePath, @serverPath, @allowList, @denyList, @apiBaseUrl,
                    @state, @currentCount, @allFilesCount, @comparedFilesCount, @equalFilesCount, @confilctFilesCount,
                    @copiedLocalFilesCount, @copiedServerFilesCount, @deletedLocalFilesCount, @deletedServerFilesCount,
                    @errorFilesCount, @ignoreFilesCount, @localFolderPath, @currentQueryFolderRelPath, @currentCopyToLocalRelPath,
                    @currentCopyToServerRelPath, @currentDeleteFromServerRelPath, @currentDeleteFromLocalRelPath);

                UPDATE sync_pairs
                SET current_sync_pair_run_id = last_insert_rowid()
                WHERE id = @syncPairId;

                DELETE FROM sync_pair_run_files WHERE sync_pair_run_id = @oldSyncPairRunId;
                DELETE FROM sync_pair_runs WHERE id = @oldSyncPairRunId;

                SELECT last_insert_rowid();
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("withSubfolders", run.WithSubfolders),
                CreateParam("isTestRun", run.IsTestRun ? 1L : 0L),
                CreateParam("requestedCancel", run.RequestedCancel ? 1L : 0L),
                CreateParam("mode", (long)run.Mode),
                CreateParam("compareType", (long)run.CompareType),
                CreateParam("conflictHandlingType", (long)run.ConflictHandlingType),
                CreateParam("name", run.Name),
                CreateParam("localFolderToken", run.LocalFolderToken),
                CreateParam("serverNamePath", run.ServerNamePath),
                CreateParam("serverPath", run.ServerPath),
                CreateParam("allowList", JsonConvert.SerializeObject(run.AllowList)),
                CreateParam("denyList", JsonConvert.SerializeObject(run.DenyList)),
                CreateParam("apiBaseUrl", run.ApiBaseUrl),
                CreateParam("state", (long)run.State),
                CreateParam("currentCount", (long)run.CurrentCount),
                CreateParam("allFilesCount", (long)run.AllFilesCount),
                CreateParam("comparedFilesCount", (long)run.ComparedFilesCount),
                CreateParam("equalFilesCount", (long)run.EqualFilesCount),
                CreateParam("confilctFilesCount", (long)run.ConflictFilesCount),
                CreateParam("copiedLocalFilesCount", (long)run.CopiedLocalFilesCount),
                CreateParam("copiedServerFilesCount", (long)run.CopiedServerFilesCount),
                CreateParam("deletedLocalFilesCount", (long)run.DeletedLocalFilesCount),
                CreateParam("deletedServerFilesCount", (long)run.DeletedServerFilesCount),
                CreateParam("errorFilesCount", (long)run.ErrorFilesCount),
                CreateParam("ignoreFilesCount", (long)run.IgnoreFilesCount),
                CreateParam("localFolderPath", run.LocalFolderPath),
                CreateParam("currentQueryFolderRelPath", run.CurrentQueryFolderRelPath),
                CreateParam("currentCopyToLocalRelPath", run.CurrentCopyToLocalRelPath),
                CreateParam("currentCopyToServerRelPath", run.CurrentCopyToServerRelPath),
                CreateParam("currentDeleteFromServerRelPath", run.CurrentDeleteFromServerRelPath),
                CreateParam("currentDeleteFromLocalRelPath", run.CurrentDeleteFromLocalRelPath),
                CreateParam("syncPairId", (long)pair.Id),
                CreateParam("oldSyncPairRunId", (long?)pair.CurrentSyncPairRunId),
            };

            long syncPairRunId = await sqlExecuteService.ExecuteScalarAsync<long>(sql, parameters);
            pair.CurrentSyncPairRunId = run.Id = (int)syncPairRunId;
        }

        private async Task UpdateSyncPairRunColumn(int syncPairRunId, string columnName, object value)
        {
            string sql = $@"
                UPDATE sync_pair_runs
                SET {columnName} = @value
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("id", syncPairRunId),
                CreateParam("value", value),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task UpdateSyncPairRunRequestCancel(int syncPairRunId)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "requested_cancel", 1L);
        }

        public async Task UpdateSyncPairRunState(int syncPairRunId, SyncPairHandlerState state)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "state", (long)state);
        }

        public async Task UpdateSyncPairRunLocalFolderPath(int syncPairRunId, string localFolderPath)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "local_folder_path", localFolderPath);
        }

        public async Task UpdateSyncPairRunCurrentQueryFolderRelPath(int syncPairRunId, string value)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "current_query_folder_rel_path", value);
        }

        public async Task UpdateSyncPairRunCurrentCopyToLocalRelPath(int syncPairRunId, string value)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "current_copy_to_local_rel_path", value);
        }

        public async Task UpdateSyncPairRunCurrentCopyToServerRelPath(int syncPairRunId, string value)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "current_copy_to_server_rel_path", value);
        }

        public async Task UpdateSyncPairRunCurrentDeleteFromServerRelPath(int syncPairRunId, string value)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "current_delete_from_server_rel_path", value);
        }

        public async Task UpdateSyncPairRunCurrentDeleteFromLocalRelPath(int syncPairRunId, string value)
        {
            await UpdateSyncPairRunColumn(syncPairRunId, "current_delete_from_local_rel_path", value);
        }

        public async Task IncreaseSyncPairRunCurrentCount(int syncPairId, int increase = 1)
        {
            const string sql = @"
                UPDATE sync_pair_runs
                SET current_count = current_count + @increase
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("id", syncPairId),
                CreateParam("increase", (long)increase),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        private static SyncPairRunFile CreateSyncPairRunFileObject(DbDataReader reader)
        {
            return new SyncPairRunFile()
            {
                Name = reader.GetString("name"),
                RelativePath = reader.GetString("relative_path"),
            };
        }

        public async Task<IList<SyncPairRunFile>> SelectSyncPairRunFiles(int syncPairRunId, SyncPairRunFileType type)
        {
            const string sql = @"
                SELECT name, relative_path
                FROM sync_pair_run_files
                WHERE sync_pair_run_id = @syncPairRunId
                    AND (type & @type > 0 OR @type = 0);
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
                CreateParam("type", (long)type),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairRunFileObject, sql, parameters);
        }

        private static SyncPairRunErrorFile CreateSyncPairRunErrorFileObject(DbDataReader reader)
        {
            return new SyncPairRunErrorFile()
            {
                Name = reader.GetString("name"),
                RelativePath = reader.GetString("relative_path"),
                Message = reader.GetString("error_message"),
                Stacktrace = reader.GetString("error_stackstrace"),
                Exception = reader.GetString("error_exception"),
            };
        }

        public async Task<IList<SyncPairRunErrorFile>> SelectSyncPairRunErrorFiles(int syncPairRunId)
        {
            const string sql = @"
                SELECT name, relative_path, error_message, error_stackstrace, error_exception
                FROM sync_pair_run_files
                WHERE sync_pair_run_id = @syncPairRunId AND type & @type > 0;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
                CreateParam("type", (long)SyncPairRunFileType.Error),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairRunErrorFileObject, sql, parameters);
        }

        public async Task ResetSyncPairRun(int syncPairRunId)
        {
            const string sql = @"
                DELETE FROM sync_pair_run_files
                WHERE sync_pair_run_id = @syncPairRunId;

                UPDATE sync_pair_runs
                SET current_count = 0,
                    all_files_count = 0,
                    error_files_count = 0,
                    equal_files_count = 0,
                    conflict_files_count = 0,
                    copied_local_files_count = 0,
                    copied_server_files_count = 0,
                    deleted_local_files_count = 0,
                    deleted_server_files_count = 0,
                    current_query_folder_rel_path = null,
                    current_copy_to_local_rel_path = null,
                    current_copy_to_server_rel_path = null,
                    current_delete_from_server_rel_path = null,
                    current_delete_from_local_rel_path = null
                WHERE id = @syncPairRunId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        private static string GetCountColumnName(SyncPairRunFileType type)
        {
            switch (type)
            {
                case SyncPairRunFileType.All:
                    return "all_files_count";

                case SyncPairRunFileType.Compared:
                    return "compared_files_count";

                case SyncPairRunFileType.Equal:
                    return "equal_files_count";

                case SyncPairRunFileType.Conflict:
                    return "conflict_files_count";

                case SyncPairRunFileType.CopiedLocal:
                    return "copied_local_files_count";

                case SyncPairRunFileType.CopiedServer:
                    return "copied_server_files_count";

                case SyncPairRunFileType.DeletedLocal:
                    return "deleted_local_files_count";

                case SyncPairRunFileType.DeletedServer:
                    return "deleted_server_files_count";

                case SyncPairRunFileType.Error:
                    return "error_files_count";

                case SyncPairRunFileType.Ignore:
                    return "ignore_files_count";

                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetInscreaseFileCount(SyncPairRunFileType type, bool increaseCurrentCount)
        {
            string countColumnName = GetCountColumnName(type);
            string increaseCurrentCountSql = increaseCurrentCount ? "current_count = current_count + 1," : "";
            return $@"
                UPDATE sync_pair_runs
                SET {increaseCurrentCountSql}
                    {countColumnName} = {countColumnName} + 1
                WHERE id = @syncPairRunId;
            ";
        }

        public async Task InsertSyncPairRunFile(int syncPairRunId, SyncPairRunFile file)
        {
            string sql = $@"
                INSERT INTO sync_pair_run_files (sync_pair_run_id, type, name, relative_path)
                VALUES (@syncPairRunId, @type, @name, @relativePath);

                {GetInscreaseFileCount(SyncPairRunFileType.All, false)}
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", syncPairRunId),
                CreateParam("type", (long)SyncPairRunFileType.All),
                CreateParam("name", file.Name),
                CreateParam("relativePath", file.RelativePath),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task SetSyncPairRunFileType(int syncPairRunId, string relativePath, SyncPairRunFileType type, bool increaseCurrentCount)
        {
            string countColumnName = GetCountColumnName(type);
            string sql = $@"
                UPDATE sync_pair_run_files
                SET type = type | @type
                WHERE sync_pair_run_id = @syncPairRunId AND relative_path = @relativePath;

                {GetInscreaseFileCount(type, increaseCurrentCount)}
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", syncPairRunId),
                CreateParam("relativePath", relativePath),
                CreateParam("type", (long)type),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task SetSyncPairRunErrorFileType(int syncPairRunId, string relativePath, Exception exception, bool increaseCurrentCount)
        {
            string sql = $@"
                UPDATE sync_pair_run_files
                SET type = type | @type
                    error_message = @errorMessage,
                    error_stackstrace = @errorStacktrace,
                    error_exception = @errorException
                WHERE sync_pair_run_id = @syncPairRunId AND relative_path = @relativePath;

                {GetInscreaseFileCount(SyncPairRunFileType.Error, increaseCurrentCount)}
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", syncPairRunId),
                CreateParam("relativePath", relativePath),
                CreateParam("type", (long)SyncPairRunFileType.Error),
                CreateParam("errorMessage", exception.Message),
                CreateParam("errorStacktrace", exception.StackTrace),
                CreateParam("errorException", exception.ToString()),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        private static SyncPairResultFile CreateSyncPairResultFileObject(DbDataReader reader)
        {
            return new SyncPairResultFile()
            {
                RelativePath = reader.GetString("relative_path"),
                LocalCompareValue = JsonConvert.DeserializeObject(reader.GetString("local_compare_value")),
                ServerCompareValue = JsonConvert.DeserializeObject(reader.GetString("server_compare_value")),
            };
        }

        public async Task<SyncPairResult> SelectLastSyncPairResult(int syncPairRunId)
        {
            const string sql = @"
                SELECT relative_path, local_compare_value, server_compare_value
                FROM sync_pair_results spr
                    JOIN sync_pair_result_files sprf ON spr.id = sprf.sync_pair_result_id
                    JOIN sync_pairs AS sp ON spr.id = sp.last_sync_pair_result_id
                WHERE sp.current_sync_pair_run_id = @syncPairRunId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
            };

            IList<SyncPairResultFile> files = await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairResultFileObject, sql, parameters);
            return new SyncPairResult(files);
        }

        private async Task InsertSyncPairResultFiles(long syncPairResultId, SyncPairResult result)
        {
            const int maxGroupSize = 5000;
            List<SyncPairResultFile> resultFilesGroup = new List<SyncPairResultFile>();
            foreach (SyncPairResultFile file in result)
            {
                resultFilesGroup.Add(file);
                if (resultFilesGroup.Count >= maxGroupSize) await InsertFiles();
            }

            if (resultFilesGroup.Count > 0) await InsertFiles();

            async Task InsertFiles()
            {
                string syncPairResultFileValuesSql = string.Join(",", result
                  .Select((_, i) => $"(@resId, @rel{i}, @local{i}, @server{i})"));
                string insertFilesSql = $@"
                    INSERT INTO sync_pair_result_files (sync_pair_result_id, relative_path, local_compare_value, server_compare_value)
                    VALUES {syncPairResultFileValuesSql};
                ";
                IEnumerable<KeyValuePair<string, object>> insertFilesParameters = new KeyValuePair<string, object>[]
                {
                    CreateParam("resId", syncPairResultId),
                }.Concat(resultFilesGroup.SelectMany((f, i) => new KeyValuePair<string, object>[]
                {
                    CreateParam($"rel{i}", f.RelativePath),
                    CreateParam($"local{i}", JsonConvert.SerializeObject(f.LocalCompareValue)),
                    CreateParam($"server{i}", JsonConvert.SerializeObject(f.ServerCompareValue)),
                }));

                await sqlExecuteService.ExecuteNonQueryAsync(insertFilesSql, insertFilesParameters);

                resultFilesGroup.Clear();
            }
        }

        public async Task InsertSyncPairResult(int syncPairRunId, SyncPairResult result)
        {
            const string lastResultSql = @"
                SELECT last_sync_pair_result_id
                FROM sync_pairs
                WHERE current_sync_pair_run_id = @syncPairRunId;
            ";
            IEnumerable<KeyValuePair<string, object>> lastResultParameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
            };
            object lastSyncPairResultIdObject = await sqlExecuteService.ExecuteScalarAsync(lastResultSql, lastResultParameters);
            long? lastSyncPairResultId = lastSyncPairResultIdObject is DBNull ? (long?)null : (long)lastSyncPairResultIdObject;

            const string insertResultSql = @"
                INSERT INTO sync_pair_results (created)
                VALUES (CURRENT_TIMESTAMP);

                SELECT last_insert_rowid();
            ";
            long newSyncPairResultId = await sqlExecuteService.ExecuteScalarAsync<long>(insertResultSql);

            await InsertSyncPairResultFiles(newSyncPairResultId, result);

            const string sql = @"
                UPDATE sync_pairs
                SET last_sync_pair_result_id = @newSyncPairResultId
                WHERE current_sync_pair_run_id = @syncPairRunId;

                DELETE FROM sync_pair_result_files WHERE sync_pair_result_id = @lastSyncPairResultId;
                DELETE FROM sync_pair_results WHERE id = @lastSyncPairResultId;
            ";

            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("newSyncPairResultId", newSyncPairResultId),
                CreateParam("syncPairRunId", (long)syncPairRunId),
                CreateParam("lastSyncPairResultId", lastSyncPairResultId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}

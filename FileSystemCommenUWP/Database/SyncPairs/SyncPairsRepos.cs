using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using Newtonsoft.Json;
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
                CREATE TABLE IF NOT EXISTS sync_pair_results (
                    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    created                 TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS sync_pair_result_files (
                    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    sync_pair_result_id     INTEGER NOT NULL REFERENCES sync_pair_results(id),
                    relative_path           TEXT NOT NULL,
                    local_compare_value     TEXT NOT NULL,
                    server_compare_value    TEXT NOT NULL,
                    created                 TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(sync_pair_result_id, relative_path)
                );

                CREATE TABLE IF NOT EXISTS sync_pair_runs (
                    id                          INTEGER PRIMARY KEY AUTOINCREMENT,
                    with_sub_folders            INTEGER NOT NULL,
                    is_test_run                 INTEGER NOT NULL,
                    requested_cancel            INTEGER NOT NULL,
                    mode                        INTEGER NOT NULL,
                    compare_type                INTEGER NOT NULL,
                    conflict_handling_type      INTEGER NOT NULL,
                    name                        TEXT NOT NULL,
                    local_folder_token          TEXT NOT NULL,
                    server_name_path            TEXT NOT NULL,
                    server_path                 TEXT NOT NULL,
                    allow_list                  TEXT NOT NULL,
                    deny_list                   TEXT NOT NULL,
                    api_base_url                TEXT NOT NULL,
                    state                       INTEGER NOT NULL,
                    current_count               INTEGER NOT NULL,
                    all_files_count             INTEGER NOT NULL,
                    compared_files_count        INTEGER NOT NULL,
                    equal_files_count           INTEGER NOT NULL,
                    conflict_files_count        INTEGER NOT NULL,
                    copied_local_files_count    INTEGER NOT NULL,
                    copied_server_files_count   INTEGER NOT NULL,
                    deleted_local_files_count   INTEGER NOT NULL,
                    deleted_server_files_count  INTEGER NOT NULL,
                    error_files_count           INTEGER NOT NULL,
                    ignore_files_count          INTEGER NOT NULL,
                    created                     TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );

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

                CREATE TABLE IF NOT EXISTS sync_pairs (
                    id                          INTEGER PRIMARY KEY AUTOINCREMENT,
                    server_id                   INTEGER NOT NULL REFERENCES servers(id),
                    current_sync_pair_run_id    INTEGER NOT NULL,
                    last_sync_pair_result_id    INTEGER NOT NULL,
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
            string[] allowList = JsonConvert.DeserializeObject<string[]>(reader.GetString("allow_list"));
            string[] denyList = JsonConvert.DeserializeObject<string[]>(reader.GetString("deny_list"));

            return new SyncPair(reader.GetString("local_folder_token"))
            {
                Id = (int)reader.GetInt64("id"),
                CurrentSyncPairRunId = (int)reader.GetInt64("current_sync_pair_run_id"),
                LastSyncPairResultId = (int)reader.GetInt64("last_sync_pair_result_id"),
                WithSubfolders = reader.GetInt64("with_subfolders") == 1L,
                Name = reader.GetString("name"),
                Mode = (SyncMode)reader.GetInt64("mode"),
                CompareType = (SyncCompareType)reader.GetInt64("compare_type"),
                ConflictHandlingType = (SyncConflictHandlingType)reader.GetInt64("conflict_handling_type"),
                AllowList = new ObservableCollection<string>(allowList),
                DenyList = new ObservableCollection<string>(denyList),
            };
        }

        public async Task<IList<SyncPair>> SelectSyncPairs(int serverId)
        {
            const string sql = @"
                SELECT id, current_sync_pair_run_id, last_sync_pair_result_id, local_folder_token, name, server_path,
                    with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list
                FROM sync_pairs
                WHERE server_id = @serverId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("id", (long)serverId),
            };

            return await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairObject, sql, parameters);
        }

        public async Task InsertSyncPair(SyncPair pair)
        {
            const string sql = @"
                INSERT INTO sync_pairs (current_sync_pair_run_id, last_sync_pair_result_id, local_folder_token, name, server_path,
                    with_subfolders, mode, compare_type, conflict_handling_type, allow_list, deny_list)
                VALUES (@currentSyncPairRunId, @lastSyncPairResultId, @name, @serverPath,
                    @withSubfolders, @mode, @compareType, @conflictHandlingType, @allowList, @denyList);

                SELECT last_insert_id();
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("currentSyncPairRunId", pair.CurrentSyncPairRunId),
                CreateParam("lastSyncPairResultId", pair.LastSyncPairResultId),
                CreateParam("local_folder_token", pair.LocalFolderToken),
                CreateParam("name", pair.Name),
                CreateParam("serverPath", pair.ServerPath),
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
                SET current_sync_pair_run_id = @currentSyncPairRunId,
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
                CreateParam("currentSyncPairRunId", pair.CurrentSyncPairRunId),
                CreateParam("lastSyncPairResultId", pair.LastSyncPairResultId),
                CreateParam("name", pair.Name),
                CreateParam("serverPath", pair.ServerPath),
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
                DELETE FROM sync_pairs WHERE id = @id;
                DELETE FROM sync_pair_run_files WHERE sync_pair_info_id = @currentSyncPairRunId;
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
                    error_files_count, ignore_files_count
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

            object nextSyncPairRunId = await sqlExecuteService.ExecuteScalarAsync(sql);
            return nextSyncPairRunId is DBNull ? (int?)null : (int)(long)nextSyncPairRunId;
        }

        public async Task InsertSyncPairRun(SyncPair pair, SyncPairRun run)
        {
            const string sql = @"
                INSERT INTO sync_pair_runs (with_sub_folders, is_test_run, requested_cancel, mode, compare_type, conflict_handling_type,
                    name, local_folder_token, server_name_path, server_path, allow_list, deny_list, api_base_url,
                    state, current_count, all_files_count, compared_files_count, equal_files_count, conflict_files_count,
                    copied_local_files_count, copied_server_files_count, deleted_local_files_count, deleted_server_files_count,
                    error_files_count, ignore_files_count)
                VALUES (@withSubfolders, @isTestRun, @requestedCancel, @mode, @compareType, @conflictHandlingType,
                    @name, @localFolderToken, @serverNamePath, @serverPath, @allowList, @denyList, @apiBaseUrl,
                    @state, @currentCount, @allFilesCount, @comparedFilesCount, @equalFilesCount, @confilctFilesCount,
                    @copiedLocalFilesCount, @copiedServerFilesCount, @deletedLocalFilesCount, @deletedServerFilesCount,
                    @errorFilesCount, @ignoreFilesCount);

                UPDATE sync_pairs
                SET current_sync_pair_run_id = last_insert_id()
                WEHRE id = @syncPairId;

                DELETE FROM sync_pair_run_files WHERE sync_pair_info_id = @oldSyncPairRunId;
                DELETE FROM sync_pair_runs WHERE id = @oldSyncPairRunId;

                SELECT last_insert_id();
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
                CreateParam("syncPairId", (long)pair.Id),
                CreateParam("oldSyncPairRunId", (long)pair.CurrentSyncPairRunId),
            };

            long syncPairRunId = await sqlExecuteService.ExecuteScalarAsync<long>(sql, parameters);
            pair.CurrentSyncPairRunId = run.Id = (int)syncPairRunId;
        }

        public async Task UpdateSyncPairRunRequestCancel(int syncPairId)
        {
            const string sql = @"
                UPDATE sync_pair_runs
                SET requested_cancel = 1
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("id", syncPairId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task UpdateSyncPairRunState(int syncPairId, SyncPairHandlerState state)
        {
            const string sql = @"
                UPDATE sync_pair_runs
                SET state = @state
                WHERE id = @id;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("id", syncPairId),
                CreateParam("state", (long)state),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
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

        //public async Task UpdateSyncPairRun(SyncPairRun run)
        //{
        //    const string sql = @"
        //        UPDATE sync_pair_runs
        //        SET with_sub_folders = @withSubfolders,
        //            is_test_run = @isTestRun,
        //            requested_cancel = @requestedCancel,
        //            mode = @mode,
        //            compare_type = @compareType,
        //            conflict_handling_type = @conflictHandlingType,
        //            name = @name,
        //            local_folder_token = @localFolderToken,
        //            server_name_path = @serverNamePath,
        //            server_path = @serverPath,
        //            allow_list = @allowList,
        //            deny_list = @denyList,
        //            api_base_url = @apiBaseUrl,
        //            state = @state,
        //            current_count = @currentCount,
        //            all_files_count = @allFilesCount,
        //            compared_files_count = @comparedFilesCount,
        //            equal_files_count = @equalFilesCount,
        //            conflict_files_count = @confilctFilesCount,
        //            copied_local_files_count = @copiedLocalFilesCount,
        //            copied_server_files_count = @copiedServerFilesCount,
        //            deleted_local_files_count = @deletedLocalFilesCount,
        //            deleted_server_files_count = @deletedServerFilesCount,
        //            error_files_count = @errorFilesCount,
        //            ignore_files_count = @ignoreFilesCount
        //        WHERE id = @id;
        //    ";
        //    IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
        //    {
        //        CreateParam("withSubfolders", run.WithSubfolders),
        //        CreateParam("isTestRun", run.IsTestRun ? 1L : 0L),
        //        CreateParam("requestedCancel", run.RequestedCancel ? 1L : 0L),
        //        CreateParam("mode", (long)run.Mode),
        //        CreateParam("compareType", (long)run.CompareType),
        //        CreateParam("conflictHandlingType", (long)run.ConflictHandlingType),
        //        CreateParam("name", run.Name),
        //        CreateParam("localFolderToken", run.LocalFolderToken),
        //        CreateParam("serverNamePath", run.ServerNamePath),
        //        CreateParam("serverPath", run.ServerPath),
        //        CreateParam("allowList", JsonConvert.SerializeObject(run.AllowList)),
        //        CreateParam("denyList", JsonConvert.SerializeObject(run.DenyList)),
        //        CreateParam("apiBaseUrl", run.ApiBaseUrl),
        //        CreateParam("state", (long)run.State),
        //        CreateParam("currentCount", (long)run.CurrentCount),
        //        CreateParam("allFilesCount", (long)run.AllFilesCount),
        //        CreateParam("comparedFilesCount", (long)run.ComparedFilesCount),
        //        CreateParam("equalFilesCount", (long)run.EqualFilesCount),
        //        CreateParam("confilctFilesCount", (long)run.ConflictFilesCount),
        //        CreateParam("copiedLocalFilesCount", (long)run.CopiedLocalFilesCount),
        //        CreateParam("copiedServerFilesCount", (long)run.CopiedServerFilesCount),
        //        CreateParam("deletedLocalFilesCount", (long)run.DeletedLocalFilesCount),
        //        CreateParam("deletedServerFilesCount", (long)run.DeletedServerFilesCount),
        //        CreateParam("errorFilesCount", (long)run.ErrorFilesCount),
        //        CreateParam("ignoreFilesCount", (long)run.IgnoreFilesCount),
        //    };

        //    await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        //}

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

        public async Task<IList<SyncPairRunErrorFile>> SelectSyncPairRunFiles(int syncPairRunId)
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

        private static string GetInscreaseFileCount(SyncPairRunFileType type)
        {
            string countColumnName = GetCountColumnName(type);
            return $@"
                UPDATE sync_pairs
                SET {countColumnName} = {countColumnName} + 1
                WHERE id = @syncPairRunId;
            ";
        }

        public async Task InsertSyncPairRunFile(int syncPairRunId, SyncPairRunFile file)
        {
            string sql = $@"
                INSERT INTO sync_pair_run_files (sync_pair_run_id, name, relative_path)
                VALUES (@syncPairRunId, @name, @relativePath);

                {GetInscreaseFileCount(SyncPairRunFileType.All)}
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", syncPairRunId),
                CreateParam("name", file.Name),
                CreateParam("relativePath", file.RelativePath),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task SetSyncPairRunFileType(int syncPairRunId, string relativePath, SyncPairRunFileType type)
        {
            string countColumnName = GetCountColumnName(type);
            string sql = $@"
                UPDATE sync_pair_run_files
                SET type = type | @type
                WHERE sync_pair_run_id = @syncPairRunId AND relative_path = @relativePath;

                {GetInscreaseFileCount(type)}
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", syncPairRunId),
                CreateParam("relativePath", relativePath),
                CreateParam("type", (long)type),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task SetSyncPairRunErrorFileType(int syncPairRunId, string relativePath, Exception exception)
        {
            string sql = $@"
                UPDATE sync_pair_run_files
                SET type = type | @type
                    error_message = @errorMessage,
                    error_stackstrace = @errorStacktrace,
                    error_exception = @errorException
                WHERE sync_pair_run_id = @syncPairRunId AND relative_path = @relativePath;

                {GetInscreaseFileCount(SyncPairRunFileType.Error)}
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
                WHERE sp.current_sync_pair_run_id = syncPairRunId;
            ";
            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("syncPairRunId", (long)syncPairRunId),
            };

            IList<SyncPairResultFile> files = await sqlExecuteService.ExecuteReadAllAsync(CreateSyncPairResultFileObject, sql, parameters);
            return new SyncPairResult(files);
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
            long lastSyncPairResultId = await sqlExecuteService.ExecuteScalarAsync<long>(lastResultSql, lastResultParameters);

            const string insertResultSql = @"
                INSERT INTO sync_pair_results (id, created)
                VALUES (DEFAULT, DEFAULT);

                SELECT last_insert_id();
            ";
            long newSyncPairResultId = await sqlExecuteService.ExecuteScalarAsync<long>(insertResultSql);

            IEnumerable<string> syncPairResultFileValuesSql = result
                .Select((_, i) => $"(@newId, @relativePath{i}, @localCompareValue{i}, @serverCompareValue{i})");
            string sql = $@"
                INSERT INTO sync_pair_results (id, created)
                VALUES (DEFAULT, DEFAULT);

                INSERT INTO sync_pair_result_files (sync_pair_result_id, relative_path, local_compare_value, server_compare_value)
                VALUES {string.Join(",", syncPairResultFileValuesSql)};

                UPDATE sync_pairs
                SET last_sync_pair_result_id = @newId;
                WHERE current_sync_pair_run_id = @syncPairRunId;

                DELETE FROM sync_pair_result_files WHERE sync_pair_result_id = @lastSyncPairResultId;
                DELETE FROM sync_pair_results WHERE id @lastSyncPairResultId;
            ";

            IEnumerable<KeyValuePair<string, object>> parameters = new KeyValuePair<string, object>[]
            {
                CreateParam("newId", newSyncPairResultId),
                CreateParam("syncPairRunId", (long)syncPairRunId),
                CreateParam("lastSyncPairResultId", lastSyncPairResultId),
            }
            .Concat(result.SelectMany((file, i) => new KeyValuePair<string, object>[]
            {
                CreateParam($"relativePath{i}", file.RelativePath),
                CreateParam($"localCompareValue{i}", file.LocalCompareValue),
                CreateParam($"serverCompareValue{i}", file.ServerCompareValue),
            }));

            await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}

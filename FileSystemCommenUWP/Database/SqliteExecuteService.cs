﻿using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Database
{
    class SqliteExecuteService: ISqlExecuteService
    {
        private readonly string connectionString;
        private SqliteConnection connection;

        public SqliteExecuteService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private async Task<SqliteCommand> GetCommand(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                connection?.Close();
                connection = new SqliteConnection(this.connectionString);
                await connection.OpenAsync();
            }

            SqliteCommand command = connection.CreateCommand();

            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            return (T)await ExecuteScalarAsync(sql, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                return await command.ExecuteScalarAsync();
            }
        }

        public async Task<T> ExecuteReadFirstAsync<T>(Func<DbDataReader, T> modelConverter, string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    return await reader.ReadAsync() ? modelConverter(reader) : default(T);
                }
            }
        }

        public async Task<IList<T>> ExecuteReadAllAsync<T>(Func<DbDataReader, T> modelConverter, string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<T> list = new List<T>();
                    while(await reader.ReadAsync())
                    {
                        list.Add(modelConverter(reader));
                    }

                    return list;
                }
            }
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}

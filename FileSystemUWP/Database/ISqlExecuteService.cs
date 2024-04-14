using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemUWP.Database
{
    interface ISqlExecuteService : IDisposable
    {
        Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<object> ExecuteScalarAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<T> ExecuteReadFirstAsync<T>(Func<DbDataReader, T> modelConverter, string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<IEnumerable<T>> ExecuteReadAllAsync<T>(Func<DbDataReader, T> modelConverter, string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);
    }
}

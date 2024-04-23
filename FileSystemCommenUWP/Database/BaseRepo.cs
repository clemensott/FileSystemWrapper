using System;
using System.Collections.Generic;

namespace FileSystemCommonUWP.Database
{
    public abstract class BaseRepo : IDisposable
    {
        protected ISqlExecuteService sqlExecuteService;

        internal BaseRepo(ISqlExecuteService sqlExecuteService)
        {
            this.sqlExecuteService = sqlExecuteService;
        }

        protected KeyValuePair<string, object> CreateParam(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        public static long? ToNullableLong(bool? value)
        {
            if (value.HasValue)
            {
                return value.Value ? 1L : 0L;
            }

            return null;
        }

        public void Dispose()
        {
            sqlExecuteService.Dispose();
        }
    }
}

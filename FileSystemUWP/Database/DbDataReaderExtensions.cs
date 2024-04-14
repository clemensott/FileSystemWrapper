using System.Data.Common;

namespace FileSystemUWP.Database
{
    static class DbDataReaderExtensions
    {
        public static bool GetBoolean(this DbDataReader reader, string name)
        {
            return reader.GetBoolean(reader.GetOrdinal(name));
        }

        public static long GetInt64(this DbDataReader reader, string name)
        {
            return reader.GetInt64(reader.GetOrdinal(name));
        }

        public static string GetString(this DbDataReader reader, string name)
        {
            return reader.GetString(reader.GetOrdinal(name));
        }
    }
}

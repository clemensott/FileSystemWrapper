using System.Data.Common;

namespace FileSystemCommonUWP.Database
{
    static class DbDataReaderExtensions
    {
        public static bool? GetBooleanFromNullableLong(this DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt64(ordinal) == 1L;
        }

        public static long GetInt64(this DbDataReader reader, string name)
        {
            return reader.GetInt64(reader.GetOrdinal(name));
        }

        public static long? GetInt64Nullable(this DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt64(reader.GetOrdinal(name));
        }

        public static string GetString(this DbDataReader reader, string name)
        {
            return reader.GetString(reader.GetOrdinal(name));
        }

        public static string GetStringNullable(this DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetString(ordinal);
        }
    }
}

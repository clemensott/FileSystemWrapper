using FileSystemCommon.Models.Configuration;
using System;
using System.IO;

namespace FileSystemWeb.Helpers
{
    static class ConfigHelper
    {
        public static Config Public { get; } = new Config()
        {
            DirectorySeparatorChar = Path.DirectorySeparatorChar,
            AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar,
        };

        /// <summary>
        /// Physical path used as base for drives folder
        /// </summary>
        public static string RootPath { get; } = Environment.GetEnvironmentVariable("FILE_SYSTEM_WRAPPER_BASE_PATH") ?? string.Empty;
   
        public static string ConnectionString { get; } = (
            Environment.GetEnvironmentVariable("FILE_SYSTEM_WRAPPER_CONNECTION_STRING") ?? $"Data Source={Path.Combine(".", "auth.db")};"
        );
    }
}

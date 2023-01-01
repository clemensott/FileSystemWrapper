using FileSystemCommon.Models.Configuration;
using System.IO;

namespace FileSystemWeb.Helpers
{
    static class ConfigHelper
    {
        public static Config Config { get; } = new Config()
        {
            DirectorySeparatorChar = Path.DirectorySeparatorChar,
            AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar,
        };
    }
}

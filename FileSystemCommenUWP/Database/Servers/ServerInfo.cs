using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommonUWP.API;

namespace FileSystemCommonUWP.Database.Servers
{
    public struct ServerInfo
    {
        public int Id { get; set; }

        public Api Api { get; set; }

        public FileSystemItemSortBy SortBy { get; set; }

        public string CurrentFolderPath { get; set; }

        public bool? RestoreIsFile { get; set; }

        public string RestoreName { get; set; }

        public string[] RestoreSortKeys { get; set; }
    }
}

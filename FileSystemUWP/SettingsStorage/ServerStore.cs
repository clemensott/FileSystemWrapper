﻿using FileSystemCommon.Models.FileSystem.Content;

namespace FileSystemUWP.SettingsStorage
{
    public class ServerStore
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string[] RawCookies { get; set; }

        public string Name { get; set; }

        public string CurrentFolderPath { get; set; }

        public FileSystemItemSortBy SortBy { get; set; }

        public FileSystemSortItemStore? RestoreFileSystemItem { get; set; }

        public SyncPairStore[] SyncPairs { get; set; }
    }
}

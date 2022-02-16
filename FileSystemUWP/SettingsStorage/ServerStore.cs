using FileSystemUWP.Picker;

namespace FileSystemUWP.SettingsStorage
{
    public class ServerStore
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string[] RawCookies { get; set; }

        public string Name { get; set; }

        public string CurrentFolderPath { get; set; }

        public FileSystemItemNameStore? RestoreFileSystemItem { get; set; }

        public SyncPairStore[] SyncPairs { get; set; }
    }
}

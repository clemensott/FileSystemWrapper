namespace FileSystemUWP.SettingsStorage
{
    public struct FileSystemSortItemStore
    {
        public bool IsFile { get; set; }

        public string Name { get; set; }

        public string[] SortKeys { get; set; }
    }
}

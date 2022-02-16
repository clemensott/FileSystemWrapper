namespace FileSystemUWP.Picker
{
    struct FileSystemItemName
    {
        public bool IsFile { get; }

        public bool IsFolder => !IsFile;

        public string Name { get; }

        public FileSystemItemName(bool isFile, string name) : this()
        {
            IsFile = isFile;
            Name = name;
        }
    }
}

using System.IO;

namespace FileSystemUWP
{
    public struct FileSystemItem
    {
        public bool IsFile { get; }

        public bool IsFolder { get; }

        public string Name { get; }

        public string Extension { get; }

        public string FullPath { get; }

        public FileSystemItem(bool isFile, string fullPath) : this()
        {
            IsFile = isFile;
            IsFolder = !isFile;
            Name = Path.GetFileName(fullPath);
            Extension = Path.GetExtension(fullPath);
            FullPath = fullPath;

            if (Name.Length == 0) Name = FullPath;
        }

        public static FileSystemItem FromFile(string path)
        {
            return new FileSystemItem(true, path);
        }

        public static FileSystemItem FromFolder(string path)
        {
            return new FileSystemItem(false, path);
        }
    }
}

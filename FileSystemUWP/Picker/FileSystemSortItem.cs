using FileSystemUWP.Models;
using System.Collections.Generic;

namespace FileSystemUWP.Picker
{
    struct FileSystemSortItem : ISortableFileSystemItem
    {
        public bool IsFile { get; }

        public bool IsFolder => !IsFile;

        public string Name { get; }

        public IReadOnlyList<string> SortKeys { get; }

        public FileSystemSortItem(bool isFile, string name, IReadOnlyList<string> sortKeys) : this()
        {
            IsFile = isFile;
            Name = name;
            SortKeys = sortKeys;
        }

        public static FileSystemSortItem FromItem(FileSystemItem item)
        {
            return new FileSystemSortItem(item.IsFile, item.Name, item.SortKeys);
        }
    }
}

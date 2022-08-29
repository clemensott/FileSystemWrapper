using System.Collections.Generic;

namespace FileSystemUWP.Picker
{
    struct FileSystemSortItem
    {
        public bool IsFile { get; }

        public bool IsFolder => !IsFile;

        public IReadOnlyList<string> SortKeys { get; set; }

        public FileSystemSortItem(bool isFile, IReadOnlyList<string> sortKeys) : this()
        {
            IsFile = isFile;
            SortKeys = sortKeys;
        }
    }
}

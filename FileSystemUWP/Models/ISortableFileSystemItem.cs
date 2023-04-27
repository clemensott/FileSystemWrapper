using System.Collections.Generic;

namespace FileSystemUWP.Models
{
    interface ISortableFileSystemItem
    {
        string Name { get; }

        IReadOnlyList<string> SortKeys { get; }
    }
}

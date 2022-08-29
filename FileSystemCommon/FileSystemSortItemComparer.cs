using FileSystemCommon.Models.FileSystem.Content;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemCommon
{
    public class FileSystemSortItemComparer : IComparer<IReadOnlyList<string>>
    {
        private static FileSystemSortItemComparer instance;

        public static FileSystemSortItemComparer Current
        {
            get
            {
                if (instance == null) instance = new FileSystemSortItemComparer();

                return instance;
            }
        }

        public FileSystemSortItemComparer()
        {
        }


        public int Compare(IReadOnlyList<string> x, IReadOnlyList<string> y)
        {
            for (int i = 0; i < x.Count && i < y.Count; i++)
            {
                int result = string.Compare(x[i], y[i]);
                if (result != 0) return result;
            }

            return x.Count.CompareTo(y.Count);
        }

        public IEnumerable<T> Sort<T>(IEnumerable<T> src,
            FileSystemItemSortDirection direction = FileSystemItemSortDirection.ASC) where T : IFileSystemSortItem
        {
            return direction == FileSystemItemSortDirection.ASC ?
                src.OrderBy(f => f.SortKeys, this) : src.OrderByDescending(f => f.SortKeys, this);
        }
    }
}

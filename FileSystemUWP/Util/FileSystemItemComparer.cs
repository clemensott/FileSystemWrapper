using FileSystemUWP.Models;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemUWP.Util
{
    class FileSystemItemComparer : IComparer<ISortableFileSystemItem>
    {
        private static FileSystemItemComparer instance;

        public static FileSystemItemComparer Current
        {
            get
            {
                if (instance == null) instance = new FileSystemItemComparer();

                return instance;
            }
        }

        public FileSystemItemComparer()
        {
        }


        public int Compare(ISortableFileSystemItem x, ISortableFileSystemItem y)
        {
            IReadOnlyList<string> xSortKeys = x.SortKeys ?? new string[0];
            IReadOnlyList<string> ySortKeys = y.SortKeys ?? new string[0];

            for (int i = 0; i < xSortKeys.Count && i < ySortKeys.Count; i++)
            {
                int result = string.Compare(xSortKeys[i], ySortKeys[i]);
                if (result != 0) return result;
            }

            if (xSortKeys.Count != ySortKeys.Count)
            {
                return xSortKeys.Count.CompareTo(ySortKeys.Count);
            }

            return x.Name.CompareTo(y.Name);
        }

        public IEnumerable<T> Sort<T>(IEnumerable<T> src) where T : ISortableFileSystemItem
        {
            return src.OrderBy(f => f, this);
        }
    }
}

using FileSystemUWP.Models;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemUWP.Util
{
    class FileSystemItemComparer : IComparer<IReadOnlyList<string>>
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


        public int Compare(IReadOnlyList<string> x, IReadOnlyList<string> y)
        {
            if (x == null) x = new string[0];
            if (y == null) y = new string[0];

            for (int i = 0; i < x.Count && i < y.Count; i++)
            {
                int result = string.Compare(x[i], y[i]);
                if (result != 0) return result;
            }

            return x.Count.CompareTo(y.Count);
        }

        public IEnumerable<FileSystemItem> Sort(IEnumerable<FileSystemItem> src)
        {
            return src.OrderBy(f => f.SortKeys, this);
        }
    }
}

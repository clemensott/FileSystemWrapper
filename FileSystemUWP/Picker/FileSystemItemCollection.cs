using FileSystemUWP.Models;
using FileSystemUWP.Util;
using StdOttStandard;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileSystemUWP.Picker
{
    class FileSystemItemCollection : ObservableCollection<FileSystemItem>
    {
        private int filesBeginIndex = 0;

        public void SetFolders(IEnumerable<FileSystemItem> folders)
        {
            int index = 0;
            foreach (FileSystemItem folder in folders)
            {
                while (true)
                {
                    int compare = index < filesBeginIndex ? Compare(folder, this[index]) : -1;

                    if (compare == 0)
                    {
                        if (!Equals(folder, this[index])) base.SetItem(index, folder);
                        index++;
                        break;
                    }
                    if (compare < 0)
                    {
                        base.InsertItem(index, folder);
                        index++;
                        filesBeginIndex++;
                        break;
                    }

                    base.RemoveItem(index);
                    filesBeginIndex--;
                }
            }

            while (index < filesBeginIndex)
            {
                base.RemoveItem(--filesBeginIndex);
            }
        }

        public void SetFiles(IEnumerable<FileSystemItem> files)
        {
            int index = filesBeginIndex;
            foreach (FileSystemItem file in files)
            {
                while (true)
                {
                    int compare = index < Count ? Compare(file, this[index]) : -1;

                    if (compare == 0)
                    {
                        if (!Equals(file, this[index])) base.SetItem(index, file);
                        index++;
                        break;
                    }
                    if (compare < 0)
                    {
                        base.InsertItem(index++, file);
                        break;
                    }

                    base.RemoveItem(index);
                }
            }

            while (index < Count)
            {
                base.RemoveItem(Count - 1);
            }
        }

        private static int Compare(FileSystemItem a, FileSystemItem b)
        {
            return a.FullPath == b.FullPath ? 0 : FileSystemItemComparer.Current.Compare(a.SortKeys, b.SortKeys);
        }

        private static bool Equals(FileSystemItem a, FileSystemItem b)
        {
            return
                a.IsFile == b.IsFile &&
                a.IsFolder == b.IsFolder &&
                a.Name == b.Name &&
                a.PathParts.BothNullOrSequenceEqual(b.PathParts) &&
                Equals(a.Permission, b.Permission) &&
                a.SortKeys.BothNullOrSequenceEqual(b.SortKeys);
        }

        public FileSystemItem? GetNearestItem(FileSystemSortItem item)
        {
            if (Count == 0) return null;

            int begin, end;
            if (item.IsFile)
            {
                begin = filesBeginIndex;
                end = Count;
            }
            else
            {
                begin = 0;
                end = filesBeginIndex;
            }

            int index;
            bool found = Search.BinarySearch(this, begin, end,
                f => FileSystemItemComparer.Current.Compare(f.SortKeys, item.SortKeys),
                out index);
            if (found && index < Count)
            {
                return this[index];
            }

            return this.LastOrDefault();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            filesBeginIndex = 0;
        }

        protected override void InsertItem(int index, FileSystemItem item)
        {
            throw new InvalidOperationException();
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new InvalidOperationException();
        }

        protected override void RemoveItem(int index)
        {
            throw new InvalidOperationException();
        }

        protected override void SetItem(int index, FileSystemItem item)
        {
            throw new InvalidOperationException();
        }
    }
}

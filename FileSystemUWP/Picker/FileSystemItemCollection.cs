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
            foreach (FileSystemItem folder in folders.OrderBy(f => f.Name))
            {
                while (true)
                {
                    int compare = index < filesBeginIndex ? folder.FullPath.CompareTo(this[index].FullPath) : -1;

                    if (compare == 0)
                    {
                        if (!Equals(folder, this[index])) base.SetItem(index, folder);
                        index++;
                        break;
                    }
                    if (compare < 0)
                    {
                        base.InsertItem(index++, folder);
                        filesBeginIndex++;
                        break;
                    }

                    base.RemoveItem(index);
                }
            }
        }

        public void SetFiles(IEnumerable<FileSystemItem> files)
        {
            int index = filesBeginIndex;
            foreach (FileSystemItem file in files.OrderBy(f => f.Name))
            {
                while (true)
                {
                    int compare = index < Count ? file.FullPath.CompareTo(this[index].FullPath) : -1;

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
        }

        private static bool Equals(FileSystemItem a, FileSystemItem b)
        {
            return
                a.IsFile == b.IsFile &&
                a.IsFolder == b.IsFolder &&
                a.Name == b.Name &&
                a.PathParts.BothNullOrSequenceEqual(b.PathParts) &&
                Equals(a.Permission, b.Permission);
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

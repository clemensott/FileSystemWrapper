using System;

namespace FileSystemUWP.Picker
{
    public class FlyoutMenuItemClickEventArgs : EventArgs
    {
        public FileSystemItem Item { get; }

        public FlyoutMenuItemClickEventArgs(FileSystemItem item)
        {
            Item = item;
        }
    }
}

using System;

namespace FileSystemUWP.FileViewers
{
    public class IsFullScreenChangedEventArgs : EventArgs
    {
        public bool IsFullScreen { get; }

        public IsFullScreenChangedEventArgs(bool isFullScreen)
        {
            IsFullScreen = isFullScreen;
        }
    }
}

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FileSystemUWP.FileViewers
{
    public class CustomViewerControls
    {
        public FrameworkElement BottomAppBar { get; }

        public FrameworkElement AbbStop { get; }

        public Frame Frame { get; }

        public CustomViewerControls(FrameworkElement bottomAppBar, FrameworkElement abbStop, Frame frame)
        {
            BottomAppBar = bottomAppBar;
            AbbStop = abbStop;
            Frame = frame;
        }
    }
}

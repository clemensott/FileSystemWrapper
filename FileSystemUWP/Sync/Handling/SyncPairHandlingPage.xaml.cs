using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
using FileSystemUWP.Sync.Handling.Mode;
using StdOttStandard.Converter.MultipleInputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.Sync.Handling
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SyncPairHandlingPage : Page
    {
        public SyncPairHandlingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = e.Parameter;

            base.OnNavigatedTo(e);
        }

        private object ModeNameConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case LocalToServerCreateOnlyModeHandler _:
                    return "Local to server (create only)";

                case LocalToServerModeHandler _:
                    return "Local to server";

                case ServerToLocalCreateOnlyModeHandler _:
                    return "Server to local (create only)";

                case ServerToLocalModeHandler _:
                    return "Server to local";

                case TwoWayModeHandler _:
                    return "Two way";
            }

            throw new ArgumentException("type not implemented", nameof(value));
        }

        private object FileCompareNameConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case HashComparer _:
                    return "SHA1 Hash";

                case SizeComparer _:
                    return "Size";

                case ExistsComparer _:
                    return "Exists";
            }

            throw new ArgumentException("type not implemented", nameof(value));
        }

        private object ConflictHandling_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case SyncConflictHandlingType.Igonre:
                    return "Ignore";

                case SyncConflictHandlingType.PreferServer:
                    return "Prefere server";

                case SyncConflictHandlingType.PreferLocal:
                    return "Prefere local";
            }

            throw new ArgumentException("type not implemented", nameof(value));
        }

        private object NotNullString_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string path = (string)value;
            if (path == null) return "<None>";
            if (path.Length == 0) return "<Base>";

            return path;
        }

        private object FilePairConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            FilePair pair = (FilePair)value;
            return pair?.RelativePath ?? "<None>";
        }

        private object MicWaiting_Convert(object sender, MultiplesInputsConvert2EventArgs args)
        {
            if ((SyncPairHandlerState?)args.Input0 == SyncPairHandlerState.WaitForStart || args.Input1 == null) return true;
            if (!IsRunning((SyncPairHandlerState?)args.Input0)) return false;

            return (int)args.Input1 == 0;
        }

        private object IsRunningConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return IsRunning((SyncPairHandlerState?)value);
        }

        private static bool IsRunning(SyncPairHandlerState? state)
        {
            return state == SyncPairHandlerState.WaitForStart ||
               state == SyncPairHandlerState.Running;
        }

        private async void TblComparedFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Compared", pairs);
        }

        private async void TblEqualFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Equal", pairs);
        }

        private async void TblIgnoreFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Ignored", pairs);
        }

        private async void TblConflictFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Conflicts", pairs);
        }

        private async void TblErrorFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Errors", pairs);
        }

        private async void TblCopiedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Copied local", pairs);
        }

        private async void TblCopiedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Copied server", pairs);
        }

        private async void TblDeletedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Deleted local", pairs);
        }

        private async void TblDeletedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Deleted server", pairs);
        }

        private static async Task ShowFileList(string title, IEnumerable<FilePair> pairs)
        {
            string message = string.Join("\r\n", pairs.Take(30).Select(p => p.RelativePath));
            if (string.IsNullOrWhiteSpace(message)) message = "<None>";

            await new MessageDialog(message, title).ShowAsync();
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbStop_Click(object sender, RoutedEventArgs e)
        {
            SyncPairHandler handler = (SyncPairHandler)DataContext;
            await handler.Cancel();
        }
    }
}

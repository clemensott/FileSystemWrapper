using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using FileSystemCommonUWP.Sync.Handling.Communication;
using StdOttStandard;
using StdOttStandard.Converter.MultipleInputs;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                case SyncMode.LocalToServerCreateOnly:
                    return "Local to server (create only)";

                case SyncMode.LocalToServer:
                    return "Local to server";

                case SyncMode.ServerToLocalCreateOnly:
                    return "Server to local (create only)";

                case SyncMode.ServerToLocal:
                    return "Server to local";

                case SyncMode.TwoWay:
                    return "Two way";
            }

            throw new ArgumentException("type not implemented", nameof(value));
        }

        private object CompareTypeNameConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case SyncCompareType.Hash:
                    return "SHA1 Hash";

                case SyncCompareType.PartialHash:
                    return "Partial SHA1 Hash";

                case SyncCompareType.Size:
                    return "Size";

                case SyncCompareType.Exists:
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

        private object FilePairInfoConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            FilePairInfo? pair = (FilePairInfo?)value;
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
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Compared", pairs);
        }

        private async void TblEqualFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Equal", pairs);
        }

        private async void TblIgnoreFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Ignored", pairs);
        }

        private async void TblConflictFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Conflicts", pairs);
        }

        private async void TblErrorFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IList<ErrorFilePairInfo> pairs = (IList<ErrorFilePairInfo>)((FrameworkElement)sender).DataContext;

            if (pairs.Count == 0)
            {
                await DialogUtils.ShowSafeAsync("<none>", "Errors");
                return;
            }

            int i = 0;
            while (true)
            {
                ErrorFilePairInfo errorPair = pairs[i];
                string title = $"Error: {i + 1} / {pairs.Count}";
                string message = $"{errorPair.Pair.RelativePath}\r\n{errorPair.Exception}";

                ContentDialogResult result = await DialogUtils.ShowContentAsync(message, title, "Cancel", "Previous", "Next", ContentDialogButton.Secondary);

                if (result == ContentDialogResult.Primary) i = StdUtils.OffsetIndex(i, pairs.Count, -1).index;
                else if (result == ContentDialogResult.Secondary) i = StdUtils.OffsetIndex(i, pairs.Count, 1).index;
                else break;
            }
        }

        private async void TblCopiedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Copied local", pairs);
        }

        private async void TblCopiedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Copied server", pairs);
        }

        private async void TblDeletedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Deleted local", pairs);
        }

        private async void TblDeletedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePairInfo> pairs = (IEnumerable<FilePairInfo>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Deleted server", pairs);
        }

        private static async Task ShowFileList(string title, IEnumerable<FilePairInfo> pairs)
        {
            string message = string.Join("\r\n", pairs.Take(30).Select(p => p.RelativePath));
            if (string.IsNullOrWhiteSpace(message)) message = "<None>";

            await DialogUtils.ShowSafeAsync(message, title);
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AbbStop_Click(object sender, RoutedEventArgs e)
        {
            SyncPairForegroundContainer container = (SyncPairForegroundContainer)DataContext;
            BackgroundTaskHelper.Current.Cancel(container.Request.RunToken);
        }
    }
}

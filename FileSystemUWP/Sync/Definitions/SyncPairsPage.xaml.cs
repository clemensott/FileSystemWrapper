using FileSystemUWP.Sync.Handling;
using StdOttStandard.Converter.MultipleInputs;
using StdOttStandard.Linq;
using StdOttUwp;
using StdOttUwp.Converters;
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

namespace FileSystemUWP.Sync.Definitions
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SyncPairsPage : Page
    {
        private readonly IDictionary<string, SingleInputConverter> handlers;
        private ViewModel viewModel;

        public SyncPairsPage()
        {
            this.InitializeComponent();

            handlers = new Dictionary<string, SingleInputConverter>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (ViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BackgroundTaskHelper.Current.AddedHandler += BackgroundTaskHelper_AddedHandler;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            BackgroundTaskHelper.Current.AddedHandler -= BackgroundTaskHelper_AddedHandler;
        }

        private void BackgroundTaskHelper_AddedHandler(object sender, SyncPairHandler e)
        {
            handlers[e.Token].Output = e;
        }

        private object SicHandler_ConvertRef(object sender, SingleInputsConvertEventArgs e)
        {
            SyncPairHandler handler;
            SingleInputConverter sic = (SingleInputConverter)sender;
            string oldToken = (string)e.OldValue;
            string newToken = (string)e.Input;

            if (!string.IsNullOrWhiteSpace(oldToken) &&
                handlers.ContainsKey(oldToken)) handlers.Remove(oldToken);

            if (string.IsNullOrWhiteSpace(newToken)) return null;

            handlers[newToken] = sic;

            return BackgroundTaskHelper.Current.TryGetHandler(newToken, out handler) ? handler : null;
        }

        private void SinHandler_Unloaded(object sender, RoutedEventArgs e)
        {
            SingleInputConverter sic = (SingleInputConverter)sender;
            string token = (string)sic.Input;

            if (!string.IsNullOrWhiteSpace(token) && handlers.ContainsKey(token)) handlers.Remove(token);
        }

        private object SicPlayCancelSymbel_Convert(object sender, SingleInputsConvertEventArgs e)
        {
            return IsRunning((SyncPairHandlerState?)e.Input);
        }

        private static bool IsRunning(SyncPairHandlerState? state)
        {
            return state == SyncPairHandlerState.WaitForStart ||
               state == SyncPairHandlerState.Running;
        }

        private async void IbnEditSync_Click(object sender, RoutedEventArgs e)
        {
            SyncPair oldSync = (SyncPair)((FrameworkElement)sender).DataContext;
            SyncPair newSync = oldSync.Clone();
            SyncPairEdit edit = new SyncPairEdit(newSync, viewModel.Api);

            Frame.Navigate(typeof(SyncEditPage), edit);

            int index;
            if (await edit.Task && viewModel.Syncs.TryIndexOf(s => s.Token == oldSync.Token, out index))
            {
                viewModel.Syncs[index] = newSync;
            }
        }

        private void IbnRunSync_Click(object sender, RoutedEventArgs e)
        {
            SyncPairHandler handler;
            SyncPair sync = (SyncPair)((FrameworkElement)sender).DataContext;

            if (BackgroundTaskHelper.Current.TryGetHandler(sync.Token, out handler) &&
                IsRunning(handler.State)) handler.Cancel();
            else BackgroundTaskHelper.Current.Start(sync, viewModel.Api);
        }

        private async void IbnRemoveSync_Click(object sender, RoutedEventArgs e)
        {
            SyncPair sync = (SyncPair)((FrameworkElement)sender).DataContext;

            if (await UwpUtils.DialogBinary(sync.Name, "Delete?", "Yes", "No")) viewModel.Syncs.Remove(sync);
        }

        private object SicVisHandling_Convert(object sender, SingleInputsConvertEventArgs e)
        {
            return e.Input != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private object MicWaiting_Convert(object sender, MultiplesInputsConvert2EventArgs args)
        {
            if ((SyncPairHandlerState?)args.Input0 == SyncPairHandlerState.WaitForStart || args.Input1 == null) return true;
            if (!IsRunning((SyncPairHandlerState?)args.Input0)) return false;

            return (int)args.Input1 == 0;
        }

        private async void TblComparedFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Compared", pairs);
        }

        private async void TblConfictFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Conficts", pairs);
        }

        private async void TblErrorFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Errors", pairs);
        }

        private async void TblEqualFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Equals", pairs);
        }

        private async void TblCopiedFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Copied", pairs);
        }

        private async void TblDeletedFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<FilePair> pairs = (IEnumerable<FilePair>)((FrameworkElement)sender).DataContext;
            await ShowFileList("Deleted", pairs);
        }

        private static async Task ShowFileList(string title, IEnumerable<FilePair> pairs)
        {
            string message = string.Join("\r\n", pairs.Select(p => p.RelativePath));
            if (string.IsNullOrWhiteSpace(message)) message = "<None>";

            await new MessageDialog(message, title).ShowAsync();
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbSyncPair_Click(object sender, RoutedEventArgs e)
        {
            SyncPair newSync = new SyncPair();
            SyncPairEdit edit = new SyncPairEdit(newSync, viewModel.Api);

            Frame.Navigate(typeof(SyncEditPage), edit);

            if (await edit.Task) viewModel.Syncs.Add(newSync);
        }

        private async void AbbRunSync_Click(object sender, RoutedEventArgs e)
        {
            await BackgroundTaskHelper.Current.Start(viewModel.Syncs, viewModel.Api);
        }
    }
}

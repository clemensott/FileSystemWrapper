using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.Sync.Handling;
using StdOttStandard.Converter.MultipleInputs;
using StdOttStandard.Linq;
using StdOttUwp;
using StdOttUwp.Converters;
using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

        private object ServerPathConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return ((PathPart[])value).GetNamePath();
        }

        private void GidSyncPair_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                FrameworkElement element = (FrameworkElement)sender;
                MenuFlyout flyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(element);
                flyout.ShowAt(element, e.GetPosition(element));
            }
        }

        private void GidSyncPair_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                FrameworkElement element = (FrameworkElement)sender;
                MenuFlyout flyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(element);
                flyout.ShowAt(element, e.GetPosition(element));
            }
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

        private void IbnHandlerDetails_Click(object sender, RoutedEventArgs e)
        {
            SyncPairHandler handler = (SyncPairHandler)((FrameworkElement)sender).DataContext;

            Frame.Navigate(typeof(SyncPairHandlingPage), handler);
        }

        private async void MfiEdit_Click(object sender, RoutedEventArgs e)
        {
            SyncPair oldSync = (SyncPair)((FrameworkElement)sender).DataContext;
            SyncPair newSync = oldSync.Clone();
            SyncPairEdit edit = new SyncPairEdit(newSync, viewModel.Api);

            Frame.Navigate(typeof(SyncEditPage), edit);

            int index;
            if (await edit.Task && viewModel.Syncs.TryIndexOf(s => s.Token == oldSync.Token, out index))
            {
                viewModel.Syncs[index] = newSync;
                App.SaveSyncPairs();
            }
        }

        private async void MfiRemove_Click(object sender, RoutedEventArgs e)
        {
            SyncPair sync = (SyncPair)((FrameworkElement)sender).DataContext;

            if (await DialogUtils.ShowTwoOptionsAsync(sync.Name, "Delete?", "Yes", "No"))
            {
                viewModel.Syncs.Remove(sync);
                App.SaveSyncPairs();
            }
        }

        private void MfiTestRun_Click(object sender, RoutedEventArgs e)
        {
            SyncPairHandler handler;
            SyncPair sync = (SyncPair)((FrameworkElement)sender).DataContext;

            if (BackgroundTaskHelper.Current.TryGetHandler(sync.Token, out handler) &&
                IsRunning(handler.State)) handler.Cancel();
            else BackgroundTaskHelper.Current.Start(sync, viewModel.Api, true);
        }

        private void MenuFlyoutSubItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuFlyoutSubItem container = (MenuFlyoutSubItem)sender;
            CreateSubItem(SyncMode.TwoWay, "Two way");
            CreateSubItem(SyncMode.ServerToLocal, "Server to local");
            CreateSubItem(SyncMode.ServerToLocalCreateOnly, "Server to local (create and override only)");
            CreateSubItem(SyncMode.LocalToServer, "Local to server");
            CreateSubItem(SyncMode.LocalToServerCreateOnly, "Local to server (create and override only)");

            void CreateSubItem(SyncMode mode, string text)
            {
                MenuFlyoutItem item = new MenuFlyoutItem()
                {
                    Text = text,
                };
                item.Click += (clickSender, clickArgs) => MfiRunWIthModeItem_Click(clickSender, clickArgs, mode);
                container.Items.Add(item);
            }
        }

        private async void MfiRunWIthModeItem_Click(object sender, RoutedEventArgs e, SyncMode mode)
        {
            SyncPair sync = UwpUtils.GetDataContext<SyncPair>(sender);

            await BackgroundTaskHelper.Current.Start(sync, viewModel.Api, mode: mode);
        }

        private void IbnRunSync_Click(object sender, RoutedEventArgs e)
        {
            SyncPairHandler handler;
            SyncPair sync = (SyncPair)((FrameworkElement)sender).DataContext;

            if (BackgroundTaskHelper.Current.TryGetHandler(sync.Token, out handler) &&
                IsRunning(handler.State)) handler.Cancel();
            else BackgroundTaskHelper.Current.Start(sync, viewModel.Api);
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

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbAddSyncPair_Click(object sender, RoutedEventArgs e)
        {
            SyncPair newSync = new SyncPair();
            SyncPairEdit edit = new SyncPairEdit(newSync, viewModel.Api);

            Frame.Navigate(typeof(SyncEditPage), edit);

            if (await edit.Task)
            {
                viewModel.Syncs.Add(newSync);
                App.SaveSyncPairs();
            }
        }

        private async void AbbRunSync_Click(object sender, RoutedEventArgs e)
        {
            await BackgroundTaskHelper.Current.Start(viewModel.Syncs, viewModel.Api);
        }
    }
}

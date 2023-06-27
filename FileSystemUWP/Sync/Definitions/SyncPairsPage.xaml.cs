using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using FileSystemCommonUWP.Sync.Handling.Communication;
using FileSystemUWP.Sync.Handling;
using StdOttStandard.Converter.MultipleInputs;
using StdOttStandard.Linq;
using StdOttUwp;
using StdOttUwp.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private Server server;
        private SyncPairsPageViewModel viewModel;

        public SyncPairsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            server = (Server)e.Parameter;

            IEnumerable<SyncPairPageSyncViewModel> syncs = server.Syncs.Select(s => new SyncPairPageSyncViewModel()
            {
                SyncPair = s,
            });
            DataContext = viewModel = new SyncPairsPageViewModel()
            {
                Syncs = new ObservableCollection<SyncPairPageSyncViewModel>(syncs),
            };

            base.OnNavigatedTo(e);

            foreach (SyncPair pair in server.Syncs)
            {
                if (!pair.IsLocalFolderLoaded)
                {
                    try
                    {
                        await pair.LoadLocalFolder();
                    }
                    catch { }
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BackgroundTaskHelper.Current.Communicator.CurrentSyncChanged += Communicator_CurrentSyncChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            BackgroundTaskHelper.Current.Communicator.CurrentSyncChanged -= Communicator_CurrentSyncChanged;
        }

        private void Communicator_CurrentSyncChanged(object sender, SyncPairForegroundContainer e)
        {
            throw new NotImplementedException();
        }

        private object ServerPathConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return ((PathPart[])value).GetNamePath(server.Api.Config.DirectorySeparatorChar);
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

        private object SicPlayCancelSymbol_Convert(object sender, SingleInputsConvertEventArgs e)
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
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);

            Frame.Navigate(typeof(SyncPairHandlingPage), sync.Run);
        }

        private async void MfiEdit_Click(object sender, RoutedEventArgs e)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);
            SyncPair newSync = sync.SyncPair.Clone();
            SyncPairEdit edit = new SyncPairEdit(newSync, server.Api, false);

            Frame.Navigate(typeof(SyncEditPage), edit);

            int index;
            if (await edit.Task && server.Syncs.TryIndexOf(s => s.Token == sync.SyncPair.Token, out index))
            {
                server.Syncs[index] = newSync;
                await App.SaveViewModel("edited sync pair");
            }
        }

        private async void MfiRemove_Click(object sender, RoutedEventArgs e)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);

            if (await DialogUtils.ShowTwoOptionsAsync(sync.SyncPair.Name ?? string.Empty, "Delete?", "Yes", "No"))
            {
                server.Syncs.Remove(sync.SyncPair);
                await App.SaveViewModel("removed sync pair");
            }
        }

        private async Task StartSyncRun(SyncPairPageSyncViewModel sync, bool isTestRun = false, SyncMode? mode = null)
        {
            if (sync.Run?.IsEnded == false) await BackgroundTaskHelper.Current.Communicator.Cancel(sync.Run.Request.RunToken);
            else await BackgroundTaskHelper.Current.Start(sync.SyncPair, server.Api, isTestRun);
        }

        private async void MfiTestRun_Click(object sender, RoutedEventArgs e)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);
            await StartSyncRun(sync, true);
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
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);

            await BackgroundTaskHelper.Current.Start(sync.SyncPair, server.Api, mode: mode);
        }

        private async void IbnRunSync_Click(object sender, RoutedEventArgs e)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);
            await StartSyncRun(sync);
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
            SyncPairEdit edit = new SyncPairEdit(newSync, server.Api, true);

            Frame.Navigate(typeof(SyncEditPage), edit);

            if (await edit.Task)
            {
                server.Syncs.Add(newSync);
                await App.SaveViewModel("added sync pair");
            }
        }

        private async void AbbRunSync_Click(object sender, RoutedEventArgs e)
        {
            await BackgroundTaskHelper.Current.Start(server.Syncs, server.Api);
        }
    }
}

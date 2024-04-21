using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using FileSystemUWP.Sync.Handling;
using StdOttStandard.Converter.MultipleInputs;
using StdOttStandard.Linq;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Storage;
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
        private readonly TimeSpan lastUpdatedSyncsMinInterval = TimeSpan.FromMilliseconds(150);

        private bool isUpdatingSyncs;
        private DateTime lastUpdatedSyncs;
        private readonly DispatcherTimer timer;
        private readonly BackgroundTaskHelper backgroundTaskHelper;
        private readonly AppDatabase database;
        private Server server;
        private SyncPairsPageViewModel viewModel;

        public SyncPairsPage()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;

            backgroundTaskHelper = BackgroundTaskHelper.Current;
            database = ((App)Application.Current).Database;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SubscribeProgress();

            server = (Server)e.Parameter;
            DataContext = viewModel = new SyncPairsPageViewModel()
            {
                Syncs = new ObservableCollection<SyncPairPageSyncViewModel>(),
            };

            await UpdateSyncs();
        }

        private async Task<StorageFolder> GetStorageFolderOrDefault(string token)
        {
            try
            {
                return await SyncLocalFolderHelper.GetLocalFolder(token);
            }
            catch
            {
                return null;
            }
        }

        private async Task<IEnumerable<SyncPairPageSyncViewModel>> LoadSyncPairs()
        {
            IList<SyncPair> syncPairs = await database.SyncPairs.SelectSyncPairs(server.Id);
            int[] syncPairRunIds = syncPairs.Select(s => s.CurrentSyncPairRunId).OfType<int>().ToArray();
            IList<SyncPairRun> syncPairRuns = await database.SyncPairs.SelectSyncPairRuns(syncPairRunIds);

            return syncPairs.Select(syncPair => new SyncPairPageSyncViewModel()
            {
                SyncPair = syncPair,
                Run = syncPairRuns.FirstOrDefault(run => run.Id == syncPair.CurrentSyncPairRunId),
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            UnsubscribeProgress();
        }

        private void SubscribeProgress()
        {
            Application.Current.EnteredBackground += OnEnteredBackground;
            Application.Current.LeavingBackground += OnLeavingBackground;

            backgroundTaskHelper.SyncProgress += BackgroundTaskHelper_SyncProgress;
            timer.Start();
        }

        private void UnsubscribeProgress()
        {
            Application.Current.EnteredBackground -= OnEnteredBackground;
            Application.Current.LeavingBackground -= OnLeavingBackground;

            backgroundTaskHelper.SyncProgress -= BackgroundTaskHelper_SyncProgress;
            timer.Stop();
        }

        private void OnEnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            backgroundTaskHelper.SyncProgress += BackgroundTaskHelper_SyncProgress;
            timer.Start();
        }

        private void OnLeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            backgroundTaskHelper.SyncProgress -= BackgroundTaskHelper_SyncProgress;
            timer.Stop();
        }

        private async void Timer_Tick(object sender, object e)
        {
            await UpdateSyncs();
        }

        private async void BackgroundTaskHelper_SyncProgress(object sender, EventArgs e)
        {
            await UwpUtils.RunSafe(() => UpdateSyncs());
        }

        private async Task UpdateSyncs()
        {
            if (isUpdatingSyncs || DateTime.Now - lastUpdatedSyncs < lastUpdatedSyncsMinInterval) return;
            isUpdatingSyncs = true;

            try
            {
                IList<SyncPair> syncPairs = await database.SyncPairs.SelectSyncPairs(server.Id);
                int[] syncPairRunIds = syncPairs.Select(s => s.CurrentSyncPairRunId).OfType<int>().ToArray();
                IList<SyncPairRun> syncPairRuns = await database.SyncPairs.SelectSyncPairRuns(syncPairRunIds);

                foreach (SyncPair syncPair in syncPairs)
                {
                    SyncPairPageSyncViewModel sync;
                    if (!viewModel.Syncs.TryFirst(s => s.SyncPair.Id == syncPair.Id, out sync))
                    {
                        sync = new SyncPairPageSyncViewModel();
                        viewModel.Syncs.Add(sync);
                    }

                    StorageFolder localFolder = await GetStorageFolderOrDefault(syncPair.LocalFolderToken);
                    syncPair.LocalFolderPath = localFolder?.Path;

                    sync.UpdateSyncPair(syncPair);
                    sync.UpdateRun(syncPairRuns.FirstOrDefault(run => run.Id == syncPair.CurrentSyncPairRunId));
                }

                foreach (SyncPairPageSyncViewModel sync in viewModel.Syncs.ToArray())
                {
                    SyncPair syncPair;
                    if (!syncPairs.TryFirst(s => s.Id == sync.SyncPair.Id, out syncPair))
                    {
                        viewModel.Syncs.Remove(sync);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Update syncs error: " + e);
            }
            finally
            {
                isUpdatingSyncs = false;
                lastUpdatedSyncs = DateTime.Now;
            }
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
            return state == SyncPairHandlerState.Loading ||
                state == SyncPairHandlerState.WaitForStart ||
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
            StorageFolder localFolder = await GetStorageFolderOrDefault(newSync.LocalFolderToken);
            string[] otherNames = viewModel.Syncs.Select(s => s.SyncPair.Name).Where(n => n != newSync.Name).ToArray();
            SyncPairEdit edit = new SyncPairEdit(newSync, otherNames, localFolder, server.Api, false);

            Frame.Navigate(typeof(SyncEditPage), edit);

            if (await edit.Task)
            {
                sync.SyncPair = newSync;
                await database.SyncPairs.UpdateSyncPair(newSync);
                SyncLocalFolderHelper.SaveLocalFolder(newSync.LocalFolderToken, edit.LocalFolder);
            }
        }

        private async void MfiRemove_Click(object sender, RoutedEventArgs e)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);

            if (await DialogUtils.ShowTwoOptionsAsync(sync.SyncPair.Name ?? string.Empty, "Delete?", "Yes", "No"))
            {
                viewModel.Syncs.Remove(sync);
                await database.SyncPairs.DeleteSyncPair(sync.SyncPair);

            }
        }

        private async Task StartSyncRun(SyncPairPageSyncViewModel sync, bool isTestRun = false, SyncMode? mode = null)
        {
            if (sync.Run?.IsEnded == false) await backgroundTaskHelper.Cancel(sync.Run);
            else sync.Run = await backgroundTaskHelper.StartSyncPairRun(sync.SyncPair, server.Api, isTestRun);
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
                item.Click += (clickSender, clickArgs) => MfiRunWithModeItem_Click(clickSender, clickArgs, mode);
                container.Items.Add(item);
            }
        }

        private async void MfiRunWithModeItem_Click(object sender, RoutedEventArgs e, SyncMode mode)
        {
            SyncPairPageSyncViewModel sync = UwpUtils.GetDataContext<SyncPairPageSyncViewModel>(sender);

            sync.Run = await backgroundTaskHelper.StartSyncPairRun(sync.SyncPair, server.Api, mode: mode);
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
            SyncPair newSync = new SyncPair(server.Id);
            string[] otherNames = viewModel.Syncs.Select(s => s.SyncPair.Name).ToArray();
            SyncPairEdit edit = new SyncPairEdit(newSync, otherNames, null, server.Api, true);

            Frame.Navigate(typeof(SyncEditPage), edit);

            if (await edit.Task)
            {
                viewModel.Syncs.Add(new SyncPairPageSyncViewModel()
                {
                    SyncPair = newSync,
                });
                await database.SyncPairs.InsertSyncPair(newSync);
                SyncLocalFolderHelper.SaveLocalFolder(newSync.LocalFolderToken, edit.LocalFolder);
            }
        }

        private async void AbbRunSync_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<SyncPair> syncs = viewModel.Syncs.Select(s => s.SyncPair);
            IEnumerable<(int syncPairId, SyncPairRun run)> containers = await backgroundTaskHelper.StartSyncPairRuns(syncs, server.Api);
            foreach ((int syncPairId, SyncPairRun run) in containers)
            {
                SyncPairPageSyncViewModel syncModel = viewModel.Syncs.FirstOrDefault(s => s.SyncPair.Id == syncPairId);
                if (syncModel != null) syncModel.Run = run;
            }
        }
    }
}

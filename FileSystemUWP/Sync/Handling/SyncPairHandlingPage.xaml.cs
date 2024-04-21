using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using StdOttStandard;
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
        private readonly TimeSpan lastUpdatedSyncsMinInterval = TimeSpan.FromMilliseconds(150);

        private bool isUpdatingSyncs;
        private DateTime lastUpdatedSyncs;
        private readonly DispatcherTimer timer;
        private readonly BackgroundTaskHelper backgroundTaskHelper;
        private readonly AppDatabase database;
        private SyncPairRun viewModel;

        public SyncPairHandlingPage()
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
            DataContext = viewModel = (SyncPairRun)e.Parameter;

            SubscribeProgress();

            await UpdateSyncs();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
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
            timer.Stop();
        }

        private async void OnLeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            backgroundTaskHelper.SyncProgress -= BackgroundTaskHelper_SyncProgress;
            timer.Start();

            await UpdateSyncs();
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
                int[] syncPairRunIds = new int[] { viewModel.Id };
                IList<SyncPairRun> syncPairRuns = await database.SyncPairs.SelectSyncPairRuns(syncPairRunIds);
                SyncPairRun run = syncPairRuns.FirstOrDefault();
                if (run == null) return;

                DataContext = viewModel = run;
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

        private object IsRunningConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return IsRunning((SyncPairHandlerState?)value);
        }

        private static bool IsRunning(SyncPairHandlerState? state)
        {
            return state == SyncPairHandlerState.Loading ||
                state == SyncPairHandlerState.WaitForStart ||
                state == SyncPairHandlerState.Running;
        }

        private async void TblComparedFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.Compared);
            await ShowFileList("Compared", pairs);
        }

        private async void TblEqualFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.Equal);
            await ShowFileList("Equal", pairs);
        }

        private async void TblIgnoreFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.Ignore);
            await ShowFileList("Ignored", pairs);
        }

        private async void TblConflictFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.Conflict);
            await ShowFileList("Conflicts", pairs);
        }

        private async void TblErrorFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IList<SyncPairRunErrorFile> pairs = await database.SyncPairs.SelectSyncPairRunErrorFiles(viewModel.Id);

            if (pairs.Count == 0)
            {
                await DialogUtils.ShowSafeAsync("<none>", "Errors");
                return;
            }

            int i = 0;
            while (true)
            {
                SyncPairRunErrorFile errorPair = pairs[i];
                string title = $"Error: {i + 1} / {pairs.Count}";
                string message = $"{errorPair.RelativePath}\r\n{errorPair.Exception}";

                ContentDialogResult result = await DialogUtils.ShowContentAsync(message, title, "Cancel", "Previous", "Next", ContentDialogButton.Secondary);

                if (result == ContentDialogResult.Primary) i = StdUtils.OffsetIndex(i, pairs.Count, -1).index;
                else if (result == ContentDialogResult.Secondary) i = StdUtils.OffsetIndex(i, pairs.Count, 1).index;
                else break;
            }
        }

        private async void TblCopiedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.CopiedLocal);
            await ShowFileList("Copied local", pairs);
        }

        private async void TblCopiedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.CopiedServer);
            await ShowFileList("Copied server", pairs);
        }

        private async void TblDeletedLocalFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.DeletedLocal);
            await ShowFileList("Deleted local", pairs);
        }

        private async void TblDeletedServerFiles_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<SyncPairRunFile> pairs = await GetSyncPairRunFiles(SyncPairRunFileType.DeletedServer);
            await ShowFileList("Deleted server", pairs);
        }

        private async Task<IEnumerable<SyncPairRunFile>> GetSyncPairRunFiles(SyncPairRunFileType type)
        {
            return await database.SyncPairs.SelectSyncPairRunFiles(viewModel.Id, type);
        }

        private static async Task ShowFileList(string title, IEnumerable<SyncPairRunFile> pairs)
        {
            string message = string.Join("\r\n", pairs.Take(30).Select(p => p.RelativePath));
            if (string.IsNullOrWhiteSpace(message)) message = "<None>";

            await DialogUtils.ShowSafeAsync(message, title);
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbStop_Click(object sender, RoutedEventArgs e)
        {
            await BackgroundTaskHelper.Current.Cancel(viewModel);
        }
    }
}

using FileSystemUWP.Sync.Handling;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace FileSystemUWP.Sync
{
    class SyncPairRunProgressUpdater
    {
        private readonly TimeSpan lastUpdatedSyncsMinInterval = TimeSpan.FromMilliseconds(150);

        private readonly BackgroundTaskHelper backgroundTaskHelper;

        private DateTime lastUpdatedSyncs;
        private int updateCount;
        private readonly object updateCountLockObj;
        private readonly SemaphoreSlim singleSem;
        private readonly Func<Task> updater;

        public SyncPairRunProgressUpdater(Func<Task> updater)
        {
            backgroundTaskHelper = BackgroundTaskHelper.Current;
            updateCountLockObj = new object();
            singleSem = new SemaphoreSlim(1);
            this.updater = updater;
        }

        public async Task Start()
        {
            Application.Current.EnteredBackground += OnEnteredBackground;
            Application.Current.LeavingBackground += OnLeavingBackground;

            backgroundTaskHelper.SyncProgress += BackgroundTaskHelper_SyncProgress;

            await CallUpdater();
        }

        public void Stop()
        {
            Application.Current.EnteredBackground -= OnEnteredBackground;
            Application.Current.LeavingBackground -= OnLeavingBackground;

            backgroundTaskHelper.SyncProgress -= BackgroundTaskHelper_SyncProgress;
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            backgroundTaskHelper.SyncProgress -= BackgroundTaskHelper_SyncProgress;
        }

        private async void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            backgroundTaskHelper.SyncProgress += BackgroundTaskHelper_SyncProgress;
            await CallUpdater();
        }

        private async void BackgroundTaskHelper_SyncProgress(object sender, EventArgs e)
        {
            await CallUpdater();
        }

        private async Task CallUpdater()
        {
            lock (updateCountLockObj)
            {
                if (updateCount > 2) return;
                updateCount++;
            }

            try
            {
                await singleSem.WaitAsync();
                TimeSpan timeUntilUpdate = lastUpdatedSyncs + lastUpdatedSyncsMinInterval - DateTime.Now;
                if (timeUntilUpdate > TimeSpan.Zero) await Task.Delay(timeUntilUpdate);

                await updater();
            }
            catch { }
            finally
            {
                lastUpdatedSyncs = DateTime.Now;
                singleSem.Release();

                lock (updateCountLockObj)
                {
                    updateCount--;
                }
            }
        }
    }
}

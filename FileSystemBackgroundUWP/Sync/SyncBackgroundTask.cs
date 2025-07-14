using FileSystemCommonUWP;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync;
using FileSystemCommonUWP.Sync.Handling;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace FileSystemBackgroundUWP.Sync
{
    public sealed class SyncBackgroundTask : IBackgroundTask
    {
        private IBackgroundTaskInstance taskInstance;
        private AppDatabase database;
        private SyncPairHandler currentSyncPairHandler;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            this.taskInstance = taskInstance;
            BackgroundTaskDeferral deferral = null;
            Timer timer = null;

            try
            {
                System.Diagnostics.Debug.WriteLine("start background task");
                deferral = taskInstance.GetDeferral();
                timer = StartTimer(taskInstance);

                database = await AppDatabase.OpenSqlite();
                // Check regualy for requested cancel

                while (true)
                {
                    int? nextSyncPairRunId = await database.SyncPairs.SelectNextSyncPairRunId();
                    if (!nextSyncPairRunId.HasValue) break;

                    await HandleRequest(nextSyncPairRunId.Value);
                }

                taskInstance.Progress = (uint)BackgroundTaskStatus.WaitStoppingA;
                await Task.Delay(TimeSpan.FromSeconds(5));
                taskInstance.Progress = (uint)BackgroundTaskStatus.Stopping;
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("background task error: " + exc);
            }
            finally
            {
                timer?.Dispose();
                deferral?.Complete();
                System.Diagnostics.Debug.WriteLine("end background task");
            }
        }

        private static async Task<AppDatabase> OpenDatabase()
        {
            StorageFile dbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("servers.db", CreationCollisionOption.OpenIfExists);
            return AppDatabase.FromSqlite(dbFile);
        }

        private async Task HandleRequest(int syncPairRunId)
        {
            try
            {
                SyncPairRun run = (await database.SyncPairs.SelectSyncPairRuns(new int[] { syncPairRunId })).First();
                SyncPairResult lastResult = await database.SyncPairs.SelectLastSyncPairResult(run.Id);
                StorageFolder localFolder = await SyncLocalFolderHelper.GetLocalFolder(run.LocalFolderToken);
                Api api = await GetAPI(run.ApiBaseUrl);

                currentSyncPairHandler = new SyncPairHandler(database, run.Id, run.WithSubfolders, run.IsTestRun,
                    run.RequestedCancel, lastResult, run.Mode, run.CompareType, run.ConflictHandlingType,
                    localFolder, run.ServerPath, run.AllowList, run.DenyList, api);

                currentSyncPairHandler.ProgressHandler.Progress += CurrentSyncPairHandler_Progress;
                await currentSyncPairHandler.Run();
            }
            catch (Exception e)
            {
                await database.SyncPairs.UpdateSyncPairRunState(syncPairRunId, SyncPairHandlerState.Error);
                System.Diagnostics.Debug.WriteLine("HandleRequest error:" + e);
            }
            finally
            {
                if (currentSyncPairHandler != null) currentSyncPairHandler.ProgressHandler.Progress += CurrentSyncPairHandler_Progress;
                currentSyncPairHandler = null;
            }
        }

        private void CurrentSyncPairHandler_Progress(object sender, EventArgs e)
        {
            TriggerProgress();
        }

        private static async Task<Api> GetAPI(string baseUrl)
        {
            Api api = new Api()
            {
                BaseUrl = baseUrl,
            };

            if (!await api.Ping()) throw new Exception("API is not reachable");

            return await api.LoadConfig() ? api : throw new Exception("Couldn't load config from API");
        }

        private Timer StartTimer(IBackgroundTaskInstance taskInstance)
        {
            return new Timer(OnTick, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(500));
        }

        private async void OnTick(object state)
        {
            TriggerProgress();

            if (currentSyncPairHandler != null && !currentSyncPairHandler.IsEnded)
            {
                bool requestedCancel = await database.SyncPairs.SelectSyncPairRunRequestedCanceled(currentSyncPairHandler.SyncPairRunId);
                if (requestedCancel) currentSyncPairHandler.Cancel();
            }
        }

        private void TriggerProgress()
        {
            taskInstance.Progress = (uint)FlipStatus((BackgroundTaskStatus)taskInstance.Progress);
        }

        private static BackgroundTaskStatus FlipStatus(BackgroundTaskStatus status)
        {
            switch (status)
            {
                case BackgroundTaskStatus.Unkown:
                case BackgroundTaskStatus.Triggered:
                case BackgroundTaskStatus.RunningB:
                case BackgroundTaskStatus.Stopped:
                    return BackgroundTaskStatus.RunningA;

                case BackgroundTaskStatus.RunningA:
                    return BackgroundTaskStatus.RunningB;

                case BackgroundTaskStatus.WaitStoppingA:
                    return BackgroundTaskStatus.WaitStoppingB;

                case BackgroundTaskStatus.WaitStoppingB:
                    return BackgroundTaskStatus.WaitStoppingA;

                case BackgroundTaskStatus.Stopping:
                    return BackgroundTaskStatus.Stopping;

                default:
                    return BackgroundTaskStatus.RunningA;
            }
        }
    }
}

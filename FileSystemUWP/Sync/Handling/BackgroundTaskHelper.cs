using FileSystemCommon;
using FileSystemCommonUWP;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace FileSystemUWP.Sync.Handling
{
    class BackgroundTaskHelper
    {
        private const string applicationBackgroundTaskBuilderName = "AppSyncFileSystemTask";
        public const string TimerBackgroundTaskBuilderName = "TimerSyncFileSystemTask";

        private static BackgroundTaskHelper instance;

        public static BackgroundTaskHelper Current
        {
            get
            {
                if (instance == null) instance = new BackgroundTaskHelper();

                return instance;
            }
        }

        private AppDatabase database;
        private ApplicationTrigger appTrigger;
        private TimeTrigger timeTrigger;

        public bool IsRunning { get; private set; } // TODO: Update this value

        private BackgroundTaskHelper()
        {
        }

        public async Task Start(AppDatabase database)
        {
            this.database = database;

            int? nextSyncPairId = await database.SyncPairs.SelectNextSyncPairRunId();
            if (nextSyncPairId.HasValue) await StartBackgroundTask();
        }

        public async Task<SyncPairRun> StartSyncPairRun(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return (await StartSyncPairRuns(new SyncPair[] { sync }, api, isTestRun, mode)).First().run;
        }

        public async Task<IEnumerable<(int syncPairId, SyncPairRun run)>> StartSyncPairRuns(IEnumerable<SyncPair> syncs, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            List<(int syncPairId, SyncPairRun run)> newContaienrs = new List<(int syncPairId, SyncPairRun run)>();
            foreach (SyncPair pair in syncs)
            {
                SyncPairRun run = FromSyncPair(pair, api, isTestRun, mode);
                newContaienrs.Add((pair.Id, run));

                await database.SyncPairs.InsertSyncPairRun(pair, run);
            }

            await StartBackgroundTask();

            return newContaienrs;
        }

        public static SyncPairRun FromSyncPair(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return new SyncPairRun()
            {
                WithSubfolders = sync.WithSubfolders,
                IsTestRun = isTestRun,
                RequestedCancel = false,
                Mode = mode ?? sync.Mode,
                CompareType = sync.CompareType,
                ConflictHandlingType = sync.ConflictHandlingType,
                Name = sync.Name,
                LocalFolderToken = sync.LocalFolderToken,
                ServerNamePath = sync.ServerPath.GetNamePath(api.Config.DirectorySeparatorChar),
                ServerPath = sync.ServerPath.LastOrDefault().Path,
                AllowList = sync.AllowList?.ToArray(),
                DenyList = sync.DenyList?.ToArray(),
                ApiBaseUrl = api.BaseUrl,
            };
        }

        private async Task StartBackgroundTask()
        {
            if (!IsRunning)
            {
                if (appTrigger == null) await RegisterAppBackgroundTask();

                await appTrigger.RequestAsync();
            }
        }

        private async Task RegisterAppBackgroundTask()
        {
            IBackgroundTaskRegistration taskRegistration;
            Guid taskRegistrationId = Settings.Current.ApplicationBackgroundTaskRegistrationId;
            if (BackgroundTaskRegistration.AllTasks.TryGetValue(taskRegistrationId, out taskRegistration))
            {
                if (taskRegistration is BackgroundTaskRegistration lastTraskRegistration &&
                    lastTraskRegistration.Trigger is ApplicationTrigger)
                {
                    appTrigger = (ApplicationTrigger)lastTraskRegistration.Trigger;
                    return;
                }

                taskRegistration.Unregister(false);
            }

            appTrigger = new ApplicationTrigger();

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = applicationBackgroundTaskBuilderName,
                IsNetworkRequested = true,
                TaskEntryPoint = "FileSystemBackgroundUWP.Sync.SyncBackgroundTask",
            };

            builder.SetTrigger(appTrigger);
            builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));

            await BackgroundExecutionManager.RequestAccessAsync();
            taskRegistration = builder.Register();
            Settings.Current.ApplicationBackgroundTaskRegistrationId = taskRegistration.TaskId;
        }

        public void RegisterTimerBackgroundTask()
        {
            Guid taskRegistrationId = Settings.Current.TimerBackgroundTaskRegistrationId;

            if (BackgroundTaskRegistration.AllTasks.ContainsKey(taskRegistrationId)) return;

            timeTrigger = new TimeTrigger(60 * 24, false);

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = applicationBackgroundTaskBuilderName,
                IsNetworkRequested = true,
            };

            builder.SetTrigger(timeTrigger);
            builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
            builder.AddCondition(new SystemCondition(SystemConditionType.BackgroundWorkCostNotHigh));

            BackgroundTaskRegistration taskRegistration = builder.Register();
            Settings.Current.TimerBackgroundTaskRegistrationId = taskRegistration.TaskId;
        }

        public async Task Cancel(SyncPairRun run)
        {
            await database.SyncPairs.UpdateSyncPairRunRequestCancel(run.Id);
            await StartBackgroundTask();
        }
    }
}

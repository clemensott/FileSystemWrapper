using FileSystemUWP.API;
using FileSystemUWP.Sync.Definitions;
using System;
using System.Collections.Generic;
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

        private ApplicationTrigger appTrigger;
        private TimeTrigger timeTrigger;
        private readonly Dictionary<string, SyncPairHandler> handlers;

        public event EventHandler<SyncPairHandler> AddedHandler;

        public bool IsRunning { get; set; }

        public Queue<SyncPairHandler> Queue { get; }

        private BackgroundTaskHelper()
        {
            handlers = new Dictionary<string, SyncPairHandler>();
            Queue = new Queue<SyncPairHandler>();
        }

        public bool TryGetHandler(string token, out SyncPairHandler handler)
        {
            return handlers.TryGetValue(token, out handler);
        }

        public Task Start(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return Start(new SyncPair[] { sync }, api, isTestRun, mode);
        }

        public async Task Start(IEnumerable<SyncPair> syncs, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            bool addedHandler = false;

            foreach (SyncPair pair in syncs)
            {
                SyncPairHandler handler;
                if (handlers.TryGetValue(pair.Token, out handler) && !handler.IsEnded) continue;

                if (!pair.IsLocalFolderLoaded)
                {
                    await pair.LoadLocalFolder();
                }
                handler = SyncPairHandler.FromSyncPair(pair, api, isTestRun, mode);

                Queue.Enqueue(handler);
                handlers[handler.Token] = handler;

                AddedHandler?.Invoke(this, handler);
                addedHandler = true;
            }

            if (!addedHandler || IsRunning) return;
            if (appTrigger == null) RegisterAppBackgroundTask();

            await appTrigger.RequestAsync();
        }

        private void RegisterAppBackgroundTask()
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
            };

            builder.SetTrigger(appTrigger);
            builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));

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
    }
}

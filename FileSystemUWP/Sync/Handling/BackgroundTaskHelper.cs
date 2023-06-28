using FileSystemCommonUWP;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.Communication;
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

        private readonly SyncPairCommunicator communicator;
        private readonly Dictionary<string, SyncPairForegroundContainer> containers;
        private ApplicationTrigger appTrigger;
        private TimeTrigger timeTrigger;

        public bool IsRunning { get; set; }


        private BackgroundTaskHelper()
        {
            communicator = SyncPairCommunicator.CreateForegroundCommunicator();
            communicator.ProgressSyncPairRun += Communicator_ProgressSyncPairRun;
            communicator.StoppedBackgroundTask += Communicator_StoppedBackgroundTask;

            SyncPairRequestInfo[] containersArray = communicator.LoadContainers();
            containers = containersArray?.ToDictionary(r => r.RunToken, r => new SyncPairForegroundContainer(r)) ?? new Dictionary<string, SyncPairForegroundContainer>();
        }

        private void Communicator_ProgressSyncPairRun(object sender, ProgressSyncPairRunEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Communicator_StoppedBackgroundTask(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task Start(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return Start(new SyncPair[] { sync }, api, isTestRun, mode);
        }

        public async Task Start(IEnumerable<SyncPair> syncs, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            foreach (SyncPair pair in syncs)
            {
                SyncPairForegroundContainer container = SyncPairForegroundContainer.FromSyncPair(pair, api, isTestRun, mode);

                containers.Add(container.Request.RunToken, container);
                communicator.SaveContainers(containers.Values.Select(c => c.Request).ToArray());
                communicator.SendRequestedSyncPair(container.Request.RunToken);
            }

            if (IsRunning) return;
            if (appTrigger == null) RegisterAppBackgroundTask();

            communicator.Start();
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

        public void Cancel(string runToken)
        {
            communicator.SendCanceledSyncPair(runToken);
        }

        public void Delete(string runToken)
        {
            communicator.SendCanceledSyncPair(runToken);
        }
    }
}

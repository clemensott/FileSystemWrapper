using FileSystemCommonUWP;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.Communication;
using StdOttStandard.Linq;
using StdOttUwp;
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

        private readonly SyncPairCommunicator communicator;
        private readonly List<SyncPairRequestInfo> requests;
        private readonly Dictionary<string, SyncPairForegroundContainer> containers;
        private ApplicationTrigger appTrigger;
        private TimeTrigger timeTrigger;

        public bool IsRunning { get; private set; }


        private BackgroundTaskHelper()
        {
            communicator = SyncPairCommunicator.CreateForegroundCommunicator();
            communicator.ProgressSyncPairRun += Communicator_ProgressSyncPairRun;
            communicator.StartedBackgroundTask += Communicator_StartedBackgroundTask;
            communicator.StoppedBackgroundTask += Communicator_StoppedBackgroundTask;

            requests = new List<SyncPairRequestInfo>();
            containers = new Dictionary<string, SyncPairForegroundContainer>();
        }

        private async void Communicator_ProgressSyncPairRun(object sender, ProgressSyncPairRunEventArgs e)
        {
            IsRunning = true;

            SyncPairForegroundContainer container;
            if (containers.TryGetValue(e.Response.RunToken, out container))
            {
                await UwpUtils.RunSafe(() => container.Response = e.Response);
            }
        }

        private void Communicator_StartedBackgroundTask(object sender, EventArgs e)
        {
            IsRunning = true;
        }

        private void Communicator_StoppedBackgroundTask(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        public async Task LoadContainers()
        {
            SyncPairRequestInfo[] requests = await communicator.LoadSyncPairRequests();
            SyncPairResponseInfo[] responses = await communicator.LoadSyncPairResponses();
            foreach (SyncPairRequestInfo request in requests.ToNotNull())
            {
                this.requests.Add(request);

                SyncPairResponseInfo response;
                SyncPairForegroundContainer container;
                if (responses != null && responses.TryFirst(r => r.RunToken == request.RunToken, out response))
                {
                    container = new SyncPairForegroundContainer(request, response);
                }
                else container = new SyncPairForegroundContainer(request);

                containers.Add(request.RunToken, container);
            }
        }

        public Task SaveRequests()
        {
            return communicator.SaveRequests(requests.ToArray());
        }

        public async Task RemoveSyncPairRuns(string token)
        {
            bool changed = false;
            while (true)
            {
                int index = requests.FindIndex(r => r.Token == token);
                if (index == -1) break;

                containers.Remove(requests[index].RunToken);
                requests.RemoveAt(index);
            }

            if (changed)
            {
                await SaveRequests();
                communicator.SendUpdatedRequestedSyncRunsPairs();
            }
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
                requests.Add(container.Request);
            }

            await SaveRequests();
            communicator.SendUpdatedRequestedSyncRunsPairs();
            communicator.Start();

            if (IsRunning) return;
            if (appTrigger == null) await RegisterAppBackgroundTask();

            await appTrigger.RequestAsync();
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

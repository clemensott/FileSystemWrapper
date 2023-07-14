using FileSystemCommonUWP;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using FileSystemCommonUWP.Sync.Handling.Communication;
using StdOttStandard.Linq;
using StdOttUwp;
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
        private readonly List<SyncPairRequestInfo> requests;
        private readonly Dictionary<string, SyncPairForegroundContainer> containers;
        private ApplicationTrigger appTrigger;
        private TimeTrigger timeTrigger;

        public bool IsRunning { get; private set; }


        private BackgroundTaskHelper()
        {
            communicator = SyncPairCommunicator.CreateForegroundCommunicator();
            communicator.ProgressSyncPairRun += Communicator_ProgressSyncPairRun;
            communicator.ProgressUpdatesSyncPairRun += Communicator_ProgressUpdatesSyncPairRun;
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

        private async void Communicator_ProgressUpdatesSyncPairRun(object sender, ProgressUpdatesSyncPairRunEventArgs e)
        {
            IsRunning = true;

            await UwpUtils.RunSafe(() =>
            {
                SyncPairForegroundContainer container = null;
                foreach (SyncPairProgressUpdate update in e.Updates)
                {
                    if (container != null && container.Response.RunToken == update.Token ||
                        containers.TryGetValue(update.Token, out container))
                    {
                        UpdateResponse(container.Response, update);
                    }
                }
            });
        }

        private void UpdateResponse(SyncPairResponseInfo response, SyncPairProgressUpdate update)
        {
            if (response == null) return;

            try
            {
                switch (update.Prop)
                {
                    case nameof(SyncPairResponseInfo.State):
                        response.State = update.State.Value;
                        return;

                    case nameof(SyncPairResponseInfo.CurrentCount):
                        response.CurrentCount = update.Number.Value;
                        return;

                    case nameof(SyncPairResponseInfo.TotalCount):
                        response.TotalCount = update.Number.Value;
                        return;

                    case nameof(SyncPairResponseInfo.AllFiles):
                        response.AddFile(update.File.Value);
                        return;

                    case nameof(SyncPairResponseInfo.ComparedFiles):
                        AddFile(response, response.ComparedFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.EqualFiles):
                        AddFile(response, response.EqualFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.IgnoreFiles):
                        AddFile(response, response.IgnoreFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.ConflictFiles):
                        AddFile(response, response.ConflictFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.CopiedLocalFiles):
                        AddFile(response, response.CopiedLocalFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.DeletedLocalFiles):
                        AddFile(response, response.DeletedLocalFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.DeletedServerFiles):
                        AddFile(response, response.DeletedServerFiles, update.Text);
                        return;

                    case nameof(SyncPairResponseInfo.ErrorFiles):
                        response.ErrorFiles.Add(update.ErrorFile.Value);
                        return;

                    case nameof(SyncPairResponseInfo.CurrentQueryFolderRelPath):
                        response.CurrentQueryFolderRelPath = update.Text;
                        return;

                    case nameof(SyncPairResponseInfo.CurrentCopyToLocalFile):
                        response.CurrentCopyToLocalFile = update.File;
                        return;

                    case nameof(SyncPairResponseInfo.CurrentCopyToServerFile):
                        response.CurrentCopyToServerFile = update.File;
                        return;

                    case nameof(SyncPairResponseInfo.CurrentDeleteFromServerFile):
                        response.CurrentDeleteFromServerFile = update.File;
                        return;

                    case nameof(SyncPairResponseInfo.CurrentDeleteFromLocalFile):
                        response.CurrentDeleteFromLocalFile = update.File;
                        return;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("UpdateResponse error: " + e);
            }
        }

        private static void AddFile(SyncPairResponseInfo response, IList<FilePairInfo> list, string key)
        {
            FilePairInfo file;
            if (response.TryGetFile(key, out file)) list.Add(file);
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

            if (containers.Values.Any(container => !container.IsEnded))
            {
                await StartBackgroundTask();
                communicator.SendRequestedProgressSyncPair();
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

        public IEnumerable<SyncPairForegroundContainer> GetContainersFromToken(string token)
        {
            return containers.Values.Where(c => c.Request.Token == token);
        }

        public async Task<SyncPairForegroundContainer> Start(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            IEnumerable<SyncPairForegroundContainer> containers = await Start(new SyncPair[] { sync }, api, isTestRun, mode);
            return containers.First();
        }

        public async Task<IEnumerable<SyncPairForegroundContainer>> Start(IEnumerable<SyncPair> syncs, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            List<SyncPairForegroundContainer> newContaienrs = new List<SyncPairForegroundContainer>();
            foreach (SyncPair pair in syncs)
            {
                SyncPairForegroundContainer container = SyncPairForegroundContainer.FromSyncPair(pair, api, isTestRun, mode);

                newContaienrs.Add(container);
                containers.Add(container.Request.RunToken, container);
                requests.Add(container.Request);
            }

            await SaveRequests();
            communicator.SendUpdatedRequestedSyncRunsPairs();

            await StartBackgroundTask();

            return newContaienrs;
        }

        private async Task StartBackgroundTask()
        {
            communicator.Start();

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

        public async Task Cancel(string runToken)
        {
            SyncPairForegroundContainer container;
            int requestIndex = requests.FindIndex(r => r.RunToken == runToken);
            if (requestIndex == -1 || !containers.TryGetValue(runToken, out container)) return;

            SyncPairRequestInfo request = requests[requestIndex];
            request.IsCanceled = true;

            requests[requestIndex] = request;
            container.Request = request;

            await SaveRequests();
            communicator.SendCanceledSyncPair(runToken);

            await StartBackgroundTask();
        }

        public void Delete(string runToken)
        {
            communicator.SendCanceledSyncPair(runToken);
        }
    }
}

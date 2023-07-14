using StdOttStandard;
using StdOttStandard.ProcessCommunication;
using StdOttUwp.ProcessCommunication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class SyncPairCommunicator : ProcessCommunicator
    {
        private const string requestsFileName = "sync_pair_communicator_requests.xml";
        private const string responsesFileName = "sync_pair_communicator_responses.xml";

        private const string backgroundReadCmdFileName = "background_sync_pair_communication.xml";
        private const string backgroundTmpReadCmdFileName = "background_sync_pair_communication.tmp";
        private const string foregroundReadCmdFileName = "foreground_sync_pair_communication.xml";
        private const string foregroundTmpReadCmdFileName = "foreground_sync_pair_communication.tmp";

        private const char syncPairRunTokensSeparator = '|';

        private const string updatedRequestedSyncPairRunsName = "updated_requested_sync_pair_runs";
        private const string canceledSyncPairRunName = "canceled_sync_pair_run";
        private const string requestedProgressSyncPairRunName = "request_progress_sync_pair_run";
        private const string progressSyncPairRunName = "progress_sync_pair_run";
        private const string progressUpdateSyncPairRunName = "progress_update_sync_pair_run";
        private const string startedBackgroundTaskName = "started_background_task";
        private const string stoppedBackgroundTaskName = "stopped_background_task";

        private long lastMessageCount = 0;
        private readonly StorageFolder folder;

        public event EventHandler<EventArgs> UpdatedRequestedSyncPairRuns;
        public event EventHandler<CanceledSyncPairRunEventArgs> CanceledSyncPairRun;
        public event EventHandler<RequestedProgressSyncPairRunEventArgs> RequestedProgressSyncPairRun;
        public event EventHandler<ProgressSyncPairRunEventArgs> ProgressSyncPairRun;
        public event EventHandler<ProgressUpdatesSyncPairRunEventArgs> ProgressUpdatesSyncPairRun;
        public event EventHandler<EventArgs> StartedBackgroundTask;
        public event EventHandler<EventArgs> StoppedBackgroundTask;

        private SyncPairCommunicator(StorageFolder folder, string readCmdsName, string writeCmdsName, string writeTmpCmdsFileName)
            : base(new FileProcessCmdPersisting(folder, readCmdsName, writeCmdsName, writeTmpCmdsFileName), 300)
        {
            this.folder = folder;
        }

        protected override async void ReceiveCommands(ICollection<ReceivedProcessCommand> cmds)
        {
            List<SyncPairProgressUpdate> updates = null;
            lastMessageCount++;

            foreach (ReceivedProcessCommand cmd in cmds)
            {
                switch (cmd.Name)
                {
                    case updatedRequestedSyncPairRunsName:
                        UpdatedRequestedSyncPairRuns?.Invoke(this, EventArgs.Empty);
                        break;

                    case canceledSyncPairRunName:
                        CanceledSyncPairRun?.Invoke(this, new CanceledSyncPairRunEventArgs(cmd.Data));
                        break;

                    case requestedProgressSyncPairRunName:
                        RequestedProgressSyncPairRun?.Invoke(this, new RequestedProgressSyncPairRunEventArgs(cmd.Data));
                        break;

                    case progressSyncPairRunName:
                        SyncPairResponseInfo response = cmd.DeserializeData<SyncPairResponseInfo>();
                        ProgressSyncPairRun?.Invoke(this, new ProgressSyncPairRunEventArgs(response));
                        break;

                    case progressUpdateSyncPairRunName:
                        if (updates == null) updates = new List<SyncPairProgressUpdate>();
                        updates.Add(cmd.DeserializeData<SyncPairProgressUpdate>());
                        break;

                    case startedBackgroundTaskName:
                        StartedBackgroundTask?.Invoke(this, EventArgs.Empty);
                        break;

                    case stoppedBackgroundTaskName:
                        if (await TryStopCommunicator(2000))
                        {
                            StoppedBackgroundTask?.Invoke(this, EventArgs.Empty);
                            StopTimer();
                        }
                        break;
                }
            }

            if (updates != null) ProgressUpdatesSyncPairRun?.Invoke(this, new ProgressUpdatesSyncPairRunEventArgs(updates));
        }

        public async Task<bool> TryStopCommunicator(int timeoutMillis = 5000)
        {
            long count = lastMessageCount;

            await Task.Delay(timeoutMillis);

            return count == lastMessageCount;
        }

        public void SendUpdatedRequestedSyncRunsPairs()
        {
            SendKeyCommand(updatedRequestedSyncPairRunsName);
        }

        public void SendCanceledSyncPair(string runToken)
        {
            SendDataCommand(canceledSyncPairRunName, runToken, $"{canceledSyncPairRunName}_{runToken}");
        }

        public void SendRequestedProgressSyncPair()
        {
            SendKeyCommand(requestedProgressSyncPairRunName);
        }

        public void SendProgessSyncPair(SyncPairResponseInfo response)
        {
            SendDataCommand(progressSyncPairRunName, response, $"{progressSyncPairRunName}_{response.RunToken}");
        }

        public void SendProgressUpdateSyncPair(SyncPairProgressUpdate update)
        {
            if (update.Action == SyncPairProgressUpdateAction.Add)
            {
                SendDataCommand(progressUpdateSyncPairRunName, update);
            }
            else
            {
                string key = $"{progressUpdateSyncPairRunName}_{update.Token}_{update.Prop}";
                SendDataCommand(progressUpdateSyncPairRunName, update, key);
            }
        }

        public void SendStartedBackgroundTask()
        {
            SendKeyCommand(stoppedBackgroundTaskName);
        }

        public void SendStoppedBackgroundTask()
        {
            SendKeyCommand(stoppedBackgroundTaskName);
        }

        public async Task<SyncPairRequestInfo[]> LoadSyncPairRequests()
        {
            IStorageItem item = await folder.TryGetItemAsync(requestsFileName);
            if (item == null || !item.IsOfType(StorageItemTypes.File)) return null;

            try
            {
                string xml = await FileIO.ReadTextAsync((StorageFile)item);
                return string.IsNullOrWhiteSpace(xml) ? null : StdUtils.XmlDeserializeText<SyncPairRequestInfo[]>(xml);
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveRequests(SyncPairRequestInfo[] requests)
        {
            StorageFile file = await folder.CreateFileAsync(requestsFileName, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, StdUtils.XmlSerialize(requests));
        }

        public async Task<SyncPairResponseInfo[]> LoadSyncPairResponses()
        {
            IStorageItem item = await folder.TryGetItemAsync(responsesFileName);
            if (item == null || !item.IsOfType(StorageItemTypes.File)) return null;

            try
            {
                string xml = await FileIO.ReadTextAsync((StorageFile)item);
                return string.IsNullOrWhiteSpace(xml) ? null : StdUtils.XmlDeserializeText<SyncPairResponseInfo[]>(xml);
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveResponses(SyncPairResponseInfo[] responses)
        {
            StorageFile file = await folder.CreateFileAsync(responsesFileName, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, StdUtils.XmlSerialize(responses));
        }

        public void Start()
        {
            StartTimer();
        }

        public Task FlushCommands()
        {
            return FlushAllCommands();
        }

        public static SyncPairCommunicator CreateBackgroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalFolder,
                backgroundReadCmdFileName, foregroundReadCmdFileName, foregroundTmpReadCmdFileName);
        }

        public static SyncPairCommunicator CreateForegroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalFolder,
                foregroundReadCmdFileName, backgroundReadCmdFileName, backgroundTmpReadCmdFileName);
        }
    }
}

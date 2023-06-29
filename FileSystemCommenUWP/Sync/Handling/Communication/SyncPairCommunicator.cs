﻿using StdOttStandard;
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
        private const string foregroundReadCmdFileName = "foreground_sync_pair_communication.xml";

        private const char syncPairRunTokensSeparator = '|';

        private const string updatedRequestedSyncPairRunsName = "updated_requested_sync_pair_runs";
        private const string canceledSyncPairRunName = "canceled_sync_pair_run";
        private const string requestedProgressSyncPairRunName = "request_progress_sync_pair_run";
        private const string progressSyncPairRunName = "progress_sync_pair_run";
        private const string startedBackgroundTaskName = "started_background_task";
        private const string stoppedBackgroundTaskName = "stopped_background_task";

        private readonly StorageFolder folder;

        public event EventHandler<EventArgs> UpdatedRequestedSyncPairRuns;
        public event EventHandler<CanceledSyncPairRunEventArgs> CanceledSyncPairRun;
        public event EventHandler<RequestedProgressSyncPairRunEventArgs> RequestedProgressSyncPairRun;
        public event EventHandler<ProgressSyncPairRunEventArgs> ProgressSyncPairRun;
        public event EventHandler<EventArgs> StartedBackgroundTask;
        public event EventHandler<EventArgs> StoppedBackgroundTask;

        private SyncPairCommunicator(StorageFolder folder, string readCmdsName, string writeCmdsName)
            : base(new FileProcessCmdPersisting(folder, readCmdsName, writeCmdsName))
        {
            this.folder = folder;
        }

        protected override void ReceiveCommand(ReceivedProcessCommand cmd)
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

                case startedBackgroundTaskName:
                    StartedBackgroundTask?.Invoke(this, EventArgs.Empty);
                    break;

                case stoppedBackgroundTaskName:
                    StoppedBackgroundTask?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        public void SendUpdatedRequestedSyncRunsPairs()
        {
            SendCommand(updatedRequestedSyncPairRunsName);
        }

        public void SendCanceledSyncPair(string runToken)
        {
            SendCommand(canceledSyncPairRunName, runToken);
        }

        public void SendRequestedProgressSyncPair()
        {
            SendCommand(requestedProgressSyncPairRunName);
        }

        public void SendProgessSyncPair(SyncPairResponseInfo response)
        {
            SendCommand(progressSyncPairRunName, response);
        }

        public void SendStartedBackgroundTask()
        {
            SendCommand(stoppedBackgroundTaskName);
        }

        public void SendStoppedBackgroundTask()
        {
            SendCommand(stoppedBackgroundTaskName);
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

        public static SyncPairCommunicator CreateBackgroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalFolder, backgroundReadCmdFileName, foregroundReadCmdFileName);
        }

        public static SyncPairCommunicator CreateForegroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalFolder, foregroundReadCmdFileName, backgroundReadCmdFileName);
        }
    }
}

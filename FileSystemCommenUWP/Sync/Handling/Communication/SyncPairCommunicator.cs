using StdOttStandard;
using StdOttUwp.BackgroundCommunication;
using System;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class SyncPairCommunicator : BackgroundCommunicator
    {
        private const string containersKey = "sync_pair_communicator_request_contianers";

        private const string backgroundReadCmdKey = "background_sync_pair_communication";
        private const string foregroundReadCmdKey = "foreground_sync_pair_communication";

        private const string requestedSyncPairRunName = "requested_sync_pair_run";
        private const string canceledSyncPairRunName = "canceled_sync_pair_run";
        private const string deletedSyncPairRunName = "deleted_sync_pair_run";
        private const string progressSyncPairRunName = "progress_sync_pair_run";
        private const string stoppedBackgroundTaskName = "stopped_background_task";

        public event EventHandler<RequestedSyncPairRunEventArgs> RequestedSyncPairRun;
        public event EventHandler<CanceledSyncPairRunEventArgs> CanceledSyncPairRun;
        public event EventHandler<DeletedSyncPairRunEventArgs> DeletedSyncPairRun;
        public event EventHandler<ProgressSyncPairRunEventArgs> ProgressSyncPairRun;
        public event EventHandler<EventArgs> StoppedBackgroundTask;

        private SyncPairCommunicator(ApplicationDataContainer dataContainer, string readCmdsName, string writeCmdsName)
            : base(dataContainer, readCmdsName, writeCmdsName)
        {
        }

        protected override void ReceiveCommand(ReceivedBackgroundCommand cmd)
        {
            switch (cmd.Name)
            {
                case requestedSyncPairRunName:
                    RequestedSyncPairRun?.Invoke(this, new RequestedSyncPairRunEventArgs(cmd.Param));
                    break;

                case canceledSyncPairRunName:
                    CanceledSyncPairRun?.Invoke(this, new CanceledSyncPairRunEventArgs(cmd.Param));
                    break;

                case deletedSyncPairRunName:
                    DeletedSyncPairRun?.Invoke(this, new DeletedSyncPairRunEventArgs(cmd.Param));
                    break;

                case progressSyncPairRunName:
                    SyncPairResponseInfo response = cmd.DeserializeParam<SyncPairResponseInfo>();
                    ProgressSyncPairRun?.Invoke(this, new ProgressSyncPairRunEventArgs(response));
                    break;

                case stoppedBackgroundTaskName:
                    StoppedBackgroundTask?.Invoke(this, EventArgs.Empty);
                    StopTimer();
                    break;
            }
        }

        public void SendRequestedSyncPair(string runToken)
        {
            SendCommand(requestedSyncPairRunName, runToken);
        }

        public void SendCanceledSyncPair(string runToken)
        {
            SendCommand(canceledSyncPairRunName, runToken);
        }

        public void SendDeletedSyncPair(string runToken)
        {
            SendCommand(deletedSyncPairRunName, runToken);
        }

        public void SendProgessSyncPair(string runToken)
        {
            SendCommand(progressSyncPairRunName, runToken);
        }

        public void SendStoppedBackgroundTask()
        {
            SendCommand(stoppedBackgroundTaskName);
        }

        public SyncPairRequestInfo[] LoadContainers()
        {
            object obj;
            if (dataContainer.Values.TryGetValue(containersKey, out obj) && obj is string)
            {
                return StdUtils.XmlDeserializeText<SyncPairRequestInfo[]>((string)obj);
            }
            return null;
        }

        public void SaveContainers(SyncPairRequestInfo[] containers)
        {
            dataContainer.Values[containersKey] = StdUtils.XmlSerialize(containers);
        }

        public void Start()
        {
            StartTimer();
        }

        public static SyncPairCommunicator CreateBackgroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalSettings, backgroundReadCmdKey, foregroundReadCmdKey);
        }

        public static SyncPairCommunicator CreateForegroundCommunicator()
        {
            return new SyncPairCommunicator(ApplicationData.Current.LocalSettings, foregroundReadCmdKey, backgroundReadCmdKey);
        }
    }
}

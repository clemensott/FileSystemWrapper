using FileSystemCommonUWP.Sync.Handling;
using FileSystemCommonUWP.Sync.Handling.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace FileSystemBackgroundUWP.Sync
{
    public sealed class SyncBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private SyncPairCommunicator communicator;
        private SyncPairRequestInfo[] requests;
        private Dictionary<string, SyncPairResponseInfo> responses;
        private SyncPairHandler currentSyncPairHandler;
        private DateTime lastSendCurrentHandlerProgress;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("start background task");
                deferral = taskInstance.GetDeferral();

                await Setup();
                while (true)
                {
                    await Handle();

                    communicator.SendStoppedBackgroundTask();
                    if (await communicator.TryStopCommunicator(10000)) break;
                }

                await DisposeCommunicator();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("background task error: " + exc);
            }
            finally
            {
                deferral?.Complete();
                System.Diagnostics.Debug.WriteLine("end background task");
            }
        }

        private async Task Setup()
        {
            SetupCommunicator();
            SyncPairResponseInfo[] responses = await communicator.LoadSyncPairResponses();
            this.responses = responses?.ToDictionary(r => r.RunToken) ?? new Dictionary<string, SyncPairResponseInfo>();

            await LoadRequests();
            communicator.Start();
        }

        private void SetupCommunicator()
        {
            communicator = SyncPairCommunicator.CreateBackgroundCommunicator();
            communicator.UpdatedRequestedSyncPairRuns += Communicator_UpdatedRequestedSyncPairRuns;
            communicator.CanceledSyncPairRun += Communicator_CanceledSyncPairRun;
            communicator.RequestedProgressSyncPairRun += Communicator_RequestedProgressSyncPairRun;
        }

        private async Task DisposeCommunicator()
        {
            communicator.UpdatedRequestedSyncPairRuns -= Communicator_UpdatedRequestedSyncPairRuns;
            communicator.CanceledSyncPairRun -= Communicator_CanceledSyncPairRun;
            communicator.RequestedProgressSyncPairRun -= Communicator_RequestedProgressSyncPairRun;

            await communicator.FlushCommands();
            communicator.Dispose();
            communicator = null;
        }

        private async Task LoadRequests()
        {
            requests = await communicator.LoadSyncPairRequests() ?? new SyncPairRequestInfo[0];

            await InitResponses();
        }

        private async Task InitResponses()
        {
            foreach (SyncPairRequestInfo request in requests)
            {
                SyncPairResponseInfo response;
                if (!responses.TryGetValue(request.RunToken, out response))
                {
                    response = new SyncPairResponseInfo()
                    {
                        RunToken = request.RunToken,
                        State = SyncPairHandlerState.WaitForStart,
                    };
                }
                else if (response.State == SyncPairHandlerState.Loading)
                {
                    response.State = SyncPairHandlerState.WaitForStart;
                };

                responses[response.RunToken] = response;
            }

            IEnumerable<string> requestRunTokens = requests.Select(r => r.RunToken);
            foreach (SyncPairResponseInfo response in responses.Values.ToArray())
            {
                if (!requestRunTokens.Contains(response.RunToken)) responses.Remove(response.RunToken);
            }

            await communicator.SaveResponses(responses.Values.ToArray());
        }

        private async void Communicator_UpdatedRequestedSyncPairRuns(object sender, EventArgs e)
        {
            await LoadRequests();
        }

        private async void Communicator_CanceledSyncPairRun(object sender, CanceledSyncPairRunEventArgs e)
        {
            await LoadRequests();
            if (e.RunToken == currentSyncPairHandler?.RunToken) await currentSyncPairHandler.Cancel();
        }

        private async void Communicator_RequestedProgressSyncPairRun(object sender, RequestedProgressSyncPairRunEventArgs e)
        {
            await communicator.SaveResponses(responses.Values.ToArray());
            foreach (SyncPairResponseInfo response in responses.Values)
            {
                communicator.SendProgessSyncPair(response);
            }
        }

        private async Task Handle()
        {
            while (true)
            {
                SyncPairRequestInfo? nextRequest = GetNextSyncPairRequest();
                if (!nextRequest.HasValue) break;

                await HandleRequest(nextRequest.Value);
                await communicator.SaveResponses(responses.Values.ToArray());
            }
        }

        private SyncPairRequestInfo? GetNextSyncPairRequest()
        {
            foreach (SyncPairRequestInfo request in requests)
            {
                SyncPairResponseInfo response;
                if (!responses.TryGetValue(request.RunToken, out response) ||
                    response.State == SyncPairHandlerState.Loading ||
                    response.State == SyncPairHandlerState.WaitForStart ||
                    response.State == SyncPairHandlerState.Running) return request;
            }
            return null;
        }

        private async Task HandleRequest(SyncPairRequestInfo request)
        {
            try
            {
                currentSyncPairHandler = await SyncPairHandler.FromSyncPairRequest(request);
                currentSyncPairHandler.Progress += OnHandlerProgress;

                await currentSyncPairHandler.Run();

                SendProgress(currentSyncPairHandler);
            }
            catch { }
            finally
            {
                if (currentSyncPairHandler != null) currentSyncPairHandler.Progress -= OnHandlerProgress;
                currentSyncPairHandler = null;
            }
        }

        private void OnHandlerProgress(object sender, EventArgs e)
        {
            SendProgress((SyncPairHandler)sender);
        }

        private async void SendProgress(SyncPairHandler handler)
        {
            if (DateTime.Now - lastSendCurrentHandlerProgress < TimeSpan.FromMilliseconds(100))
            {
                await Task.Delay(110);
                if (DateTime.Now - lastSendCurrentHandlerProgress < TimeSpan.FromMilliseconds(100)) return;
            }

            lastSendCurrentHandlerProgress = DateTime.Now;

            SyncPairResponseInfo response = handler.ToResponse();
            responses[response.RunToken] = response;
            communicator.SendProgessSyncPair(response);
        }
    }
}

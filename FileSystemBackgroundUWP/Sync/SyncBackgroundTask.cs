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

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("start background task");
                deferral = taskInstance.GetDeferral();

                await Setup();
                await Handle();
                await Dispose();
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
            await LoadRequests();

            SyncPairResponseInfo[] responses = await communicator.LoadSyncPairResponses();
            this.responses = responses?.ToDictionary(r => r.RunToken) ?? new Dictionary<string, SyncPairResponseInfo>();
        }

        private async Task Dispose()
        {
            communicator.SendStoppedBackgroundTask();

            await Task.Delay(2000);

            DisposeCommunicator();
        }

        private void SetupCommunicator()
        {
            communicator = SyncPairCommunicator.CreateBackgroundCommunicator();
            communicator.UpdatedRequestedSyncPairRuns += Communicator_UpdatedRequestedSyncPairRuns;
            communicator.CanceledSyncPairRun += Communicator_CanceledSyncPairRun;
            communicator.RequestedProgressSyncPairRun += Communicator_RequestedProgressSyncPairRun;
            communicator.Start();
        }

        private void DisposeCommunicator()
        {
            communicator.UpdatedRequestedSyncPairRuns -= Communicator_UpdatedRequestedSyncPairRuns;
            communicator.CanceledSyncPairRun -= Communicator_CanceledSyncPairRun;
            communicator.RequestedProgressSyncPairRun -= Communicator_RequestedProgressSyncPairRun;
            communicator.Dispose();
            communicator = null;
        }

        private async Task LoadRequests()
        {
            requests = await communicator.LoadSyncPairRequests() ?? new SyncPairRequestInfo[0];
        }

        private async void Communicator_UpdatedRequestedSyncPairRuns(object sender, EventArgs e)
        {
            await LoadRequests();
        }

        private async void Communicator_CanceledSyncPairRun(object sender, CanceledSyncPairRunEventArgs e)
        {
            if (e.RunToken == currentSyncPairHandler.RunToken) await currentSyncPairHandler?.Cancel();
            await LoadRequests();
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
            SyncPairHandler handler = null;
            try
            {
                handler = await SyncPairHandler.FromSyncPairRequest(request);
                handler.Progress += OnHandlerProgress;

                await handler.Run();
            }
            catch { }
            finally
            {
                if (handler != null) handler.Progress -= OnHandlerProgress;
            }
        }

        private void OnHandlerProgress(object sender, EventArgs e)
        {
            SyncPairHandler handler = (SyncPairHandler)sender;
            communicator.SendProgessSyncPair(SyncPairResponseInfo.FromHandler(handler));
        }

        private async Task FakeSyncPairRun(SyncPairRequestInfo request)
        {
            List<SyncPairResponseInfo> list = new List<SyncPairResponseInfo>();
            SyncPairResponseInfo res = new SyncPairResponseInfo()
            {
                RunToken = request.RunToken,
                State = SyncPairHandlerState.WaitForStart,
                CurrentCount = 0,
                TotalCount = 0,
                ComparedFiles = CreatePairs(),
                EqualFiles = CreatePairs(),
                IgnoreFiles = CreatePairs(),
                ConflictFiles = CreatePairs(),
                CopiedLocalFiles = CreatePairs(),
                CopiedServerFiles = CreatePairs(),
                DeletedLocalFiles = CreatePairs(),
                DeletedServerFiles = CreatePairs(),
                ErrorFiles = new ErrorFilePairInfo[0],
                CurrentQueryFolderRelPath = "",
                CurrentCopyToLocalFile = null,
                CurrentCopyToServerFile = null,
                CurrentDeleteFromServerFile = null,
                CurrentDeleteFromLocalFile = null,
            };
            list.Add(res);

            res.State = SyncPairHandlerState.Running;
            res.CurrentQueryFolderRelPath = "some/folder/to/query/1";
            list.Add(res);

            res.TotalCount = 10;
            res.CurrentQueryFolderRelPath = "some/folder/to/query/2";
            list.Add(res);

            res.CurrentCount = 5;
            res.TotalCount = 30;
            res.ComparedFiles = CreatePairs(5);
            res.EqualFiles = CreatePairs(3);
            res.IgnoreFiles = CreatePairs(2);
            res.ConflictFiles = CreatePairs(1);
            res.CopiedLocalFiles = CreatePairs(1);
            res.ErrorFiles = new ErrorFilePairInfo[] { new ErrorFilePairInfo() { File = CreatePair(), Exception = "Exception", Message = "Message", Stacktrace = "Stacktrace" } };
            res.CurrentQueryFolderRelPath = "some/folder/to/query/3";
            res.CurrentCopyToLocalFile = CreatePair("copy_to_local_1");
            list.Add(res);

            res.CurrentCount = 10;
            res.TotalCount = 40;
            res.ComparedFiles = CreatePairs(9);
            res.CopiedServerFiles = CreatePairs(6);
            res.DeletedLocalFiles = CreatePairs(3);
            res.DeletedServerFiles = CreatePairs(4);
            res.ErrorFiles = new ErrorFilePairInfo[] {
                new ErrorFilePairInfo() { File = CreatePair(), Exception = "Exception", Message = "Message", Stacktrace = "Stacktrace" },
                new ErrorFilePairInfo() { File = CreatePair(), Exception = "Exception", Message = "Message", Stacktrace = "Stacktrace" },
            };
            res.CurrentQueryFolderRelPath = "";
            res.CurrentCopyToLocalFile = CreatePair("copy_to_local_1");
            list.Add(res);

            res.CurrentCount = 10;
            res.TotalCount = 40;
            res.IgnoreFiles = CreatePairs(5);
            res.CurrentQueryFolderRelPath = null;
            res.CurrentCopyToLocalFile = CreatePair("copy_to_local_1");
            res.CurrentCopyToServerFile = CreatePair("copy_to_server_1");
            res.CurrentDeleteFromServerFile = CreatePair("delete_from_server_1");
            res.CurrentDeleteFromLocalFile = CreatePair("delete_from_local_1");
            list.Add(res);

            res.CurrentCount = 20;
            res.TotalCount = 40;
            res.IgnoreFiles = CreatePairs(10);
            res.CurrentCopyToLocalFile = CreatePair("copy_to_local_2");
            res.CurrentCopyToServerFile = CreatePair("copy_to_server_2");
            res.CurrentDeleteFromServerFile = CreatePair("delete_from_server_2");
            res.CurrentDeleteFromLocalFile = CreatePair("delete_from_local_1");
            list.Add(res);

            res.CurrentCount = 30;
            res.TotalCount = 40;
            res.CurrentCopyToLocalFile = CreatePair("copy_to_local_3");
            res.CurrentCopyToServerFile = CreatePair("copy_to_server_2");
            res.CurrentDeleteFromServerFile = CreatePair("delete_from_server_4");
            res.CurrentDeleteFromLocalFile = CreatePair("delete_from_local_2");
            list.Add(res);

            res.CurrentCount = 35;
            res.TotalCount = 40;
            res.CurrentCopyToLocalFile = null;
            res.CurrentCopyToServerFile = null;
            res.CurrentDeleteFromServerFile = null;
            res.CurrentDeleteFromLocalFile = CreatePair("delete_from_local_2");
            list.Add(res);

            res.State = SyncPairHandlerState.Finished;
            res.CurrentCount = 40;
            res.TotalCount = 40;
            res.ComparedFiles = CreatePairs(40);
            res.EqualFiles = CreatePairs(29);
            res.IgnoreFiles = CreatePairs(12);
            res.ConflictFiles = CreatePairs(3);
            res.CopiedLocalFiles = CreatePairs(4);
            res.CopiedServerFiles = CreatePairs(8);
            res.DeletedLocalFiles = CreatePairs(21);
            res.DeletedServerFiles = CreatePairs(8);
            res.CurrentDeleteFromLocalFile = null;
            list.Add(res);

            foreach (var response in list)
            {
                responses[response.RunToken] = response;
                communicator.SendProgessSyncPair(response);

                await Task.Delay(StdOttStandard.StdUtils.Random.Next(300, 2000));
            }
        }

        private FilePairInfo[] CreatePairs(int count = 0)
        {
            return Enumerable.Range(0, count).Select(_ => CreatePair()).ToArray();
        }

        private FilePairInfo CreatePair(string name = "file")
        {
            return new FilePairInfo()
            {
                Name = name,
                RelativePath = "relative/path/to/" + name,
                ServerFullPath = "https://www.server.com/relative/path/to/" + name,
                ServerFileExists = true,
                LocalFilePath = @"C:\local\path\to\" + name,
            };
        }
    }
}

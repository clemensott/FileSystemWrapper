﻿using FileSystemCommon;
using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class SyncPairForegroundContainer : INotifyPropertyChanged
    {
        private SyncPairResponseInfo sync;

        public SyncPairRequestInfo Request { get; }

        public SyncPairResponseInfo Response
        {
            get => sync;
            private set
            {
                if (Equals(value, sync)) return;

                sync = value;
                OnPropertyChanged(nameof(Response));
            }
        }

        public bool IsEnded => Response.State == SyncPairHandlerState.Finished ||
            Response.State == SyncPairHandlerState.Error ||
            Response.State == SyncPairHandlerState.Canceled;

        public SyncPairForegroundContainer(SyncPairRequestInfo request)
        {
            Request = request;
            Response = new SyncPairResponseInfo()
            {
                State = SyncPairHandlerState.Loading,
            };
        }

        public static SyncPairForegroundContainer FromSyncPair(SyncPair sync, Api api, bool isTestRun = false, SyncMode? mode = null)
        {
            return new SyncPairForegroundContainer(new SyncPairRequestInfo()
            {
                RunToken = Guid.NewGuid().ToString(),
                Token = sync.Token,
                ResultToken = sync.ResultToken,
                Mode = mode ?? sync.Mode,
                CompareType = sync.CompareType,
                ConflictHandlingType = sync.ConflictHandlingType,
                WithSubfolders = sync.WithSubfolders,
                Name = sync.Name,
                ServerNamePath = sync.ServerPath.GetNamePath(api.Config.DirectorySeparatorChar),
                ServerPath = sync.ServerPath.LastOrDefault().Path,
                Whitelist = sync.Whitelist.ToArray(),
                Blacklist = sync.Blacklist.ToArray(),
                IsTestRun = isTestRun,
                ApiBaseUrl = api.BaseUrl,
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

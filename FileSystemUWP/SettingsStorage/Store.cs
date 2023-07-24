using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemUWP.Controls;
using FileSystemUWP.Models;
using FileSystemUWP.Picker;
using StdOttStandard;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemUWP.SettingsStorage
{
    public class Store
    {
        public int CurrentServerIndex { get; set; }

        public ServerStore[] Servers { get; set; }

        internal static async Task LoadInto(string path, ViewModel viewModel)
        {
            Store store = await Task.Run(() => StdUtils.XmlDeserializeFile<Store>(path));
            LoadStoreIntoViewModel(store, viewModel);
        }

        private static ViewModel LoadStoreIntoViewModel(Store store, ViewModel viewModel)
        {
            store.Servers?.Select(server => CreateServerExplorer(server, viewModel.BackgroundOperations)).ForEach(viewModel.Servers.Add);
            viewModel.CurrentServer = viewModel.Servers.ElementAtOrDefault(store.CurrentServerIndex);

            return viewModel;
        }

        private static Server CreateServerExplorer(ServerStore server, BackgroundOperations backgroundOperations)
        {
            IEnumerable<SyncPair> pairs = server.SyncPairs?.Select(CreateSyncPair);
            return new Server(backgroundOperations, pairs)
            {
                Api = new Api()
                {
                    Name = server.Name,
                    BaseUrl = server.BaseUrl,
                    Username = server.Username,
                    RawCookies = server.RawCookies,
                },
                SortBy = server.SortBy,
                CurrentFolderPath = server.CurrentFolderPath,
                RestoreFileSystemItem = CreateFileSystemItemName(server.RestoreFileSystemItem),
            };
        }

        private static SyncPair CreateSyncPair(SyncPairStore sync)
        {
            return new SyncPair(sync.Token, sync.ResultToken)
            {
                WithSubfolders = sync.WithSubfolders,
                Name = sync.Name,
                ServerPath = sync.ServerPath,
                Mode = sync.Mode,
                CompareType = sync.CompareType,
                ConflictHandlingType = sync.ConflictHandlingType,
                Whitelist = sync.Whitelist != null ? new ObservableCollection<string>(sync.Whitelist) : null,
                Blacklist = sync.Blacklist != null ? new ObservableCollection<string>(sync.Blacklist) : null,
            };
        }

        private static FileSystemSortItem? CreateFileSystemItemName(FileSystemSortItemStore? item)
        {
            if (item.HasValue)
            {
                return new FileSystemSortItem(item.Value.IsFile, item.Value.Name, item.Value.SortKeys?.ToList().AsReadOnly());
            }
            return null;
        }

        internal static async Task Save(string path, ViewModel viewModel)
        {
            Store store = CreateStore(viewModel);
            await Task.Run(() =>
            {
                StdUtils.XmlSerialize(path, store);
            });

            foreach (Server server in viewModel.Servers)
            {
                server.Syncs.SaveLocalFolders();
            }
        }

        private static Store CreateStore(ViewModel viewModel)
        {
            return new Store()
            {
                CurrentServerIndex = viewModel.Servers.IndexOf(viewModel.CurrentServer),
                Servers = viewModel.Servers.Select(CreateServerStore).ToArray(),
            };
        }

        private static ServerStore CreateServerStore(Server viewModel)
        {
            return new ServerStore()
            {
                Name = viewModel.Api.Name,
                BaseUrl = viewModel.Api.BaseUrl,
                Username = viewModel.Api.Username,
                RawCookies = viewModel.Api.RawCookies,
                CurrentFolderPath = viewModel.CurrentFolderPath,
                SortBy = viewModel.SortBy,
                RestoreFileSystemItem = CreateFileSystemItemNameStore(viewModel.RestoreFileSystemItem),
                SyncPairs = viewModel.Syncs.Select(CreateSyncPairStore).ToArray(),
            };
        }

        private static SyncPairStore CreateSyncPairStore(SyncPair pair)
        {
            return new SyncPairStore()
            {
                Token = pair.Token,
                ResultToken = pair.ResultToken,
                WithSubfolders = pair.WithSubfolders,
                Name = pair.Name,
                ServerPath = pair.ServerPath?.ToArray(),
                Mode = pair.Mode,
                CompareType = pair.CompareType,
                ConflictHandlingType = pair.ConflictHandlingType,
                Whitelist = pair.Whitelist?.ToArray(),
                Blacklist = pair.Blacklist?.ToArray(),
            };
        }

        private static FileSystemSortItemStore? CreateFileSystemItemNameStore(FileSystemSortItem? item)
        {
            if (item.HasValue)
            {
                return new FileSystemSortItemStore()
                {
                    IsFile = item.Value.IsFile,
                    Name = item.Value.Name,
                    SortKeys = item.Value.SortKeys?.ToArray(),
                };
            }
            return null;
        }
    }
}

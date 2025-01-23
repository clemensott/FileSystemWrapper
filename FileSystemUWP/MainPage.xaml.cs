using FileSystemCommonUWP.API;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemUWP.API;
using FileSystemUWP.Models;
using StdOttUwp;
using StdOttUwp.ApplicationDataObjects;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FileSystemCommonUWP;
using FileSystemCommonUWP.Database;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly AppDatabase database;
        private ViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();

            database = ((App)Application.Current).Database;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel = (ViewModel)e.Parameter;

            if (e.NavigationMode == NavigationMode.Back)
            {
                viewModel.CurrentServer = null;
            }

            DataContext = viewModel;

            base.OnNavigatedTo(e);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentServer != null)
            {
                Frame.Navigate(typeof(ServerExplorerPage), viewModel.CurrentServer);
            }
            else
            {
                ((ListView)sender).SelectionChanged += LvwServers_SelectionChanged;
            }
        }

        private void ListView_Unloaded(object sender, RoutedEventArgs e)
        {
            ((ListView)sender).SelectionChanged -= LvwServers_SelectionChanged;
        }

        private void LvwServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Frame.Navigate(typeof(ServerExplorerPage), e.AddedItems[0]);
            }
        }

        private async void IbnEditServer_Click(object sender, RoutedEventArgs e)
        {
            Server server = UwpUtils.GetDataContext<Server>(sender);
            Api newApi = server.Api.Clone();
            ApiEdit edit = new ApiEdit(newApi, false);

            Frame.Navigate(typeof(AuthPage), edit);

            if (await edit.Task)
            {
                server.Api = newApi;
                await database.Servers.UpdateServer(server.ToInfo());
            }
        }

        private async void IbnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            Server server = UwpUtils.GetDataContext<Server>(sender);
            bool delete = await DialogUtils.ShowTwoOptionsAsync(server.Api.Name, "Delete Server?", "Yes", "No");
            if (delete)
            {
                viewModel.Servers.Remove(server);
                await database.Servers.DeleteServer(server.ToInfo());
            }
        }

        private async void AbbAddServer_Click(object sender, RoutedEventArgs e)
        {
            Api newApi = new Api();
            ApiEdit edit = new ApiEdit(newApi, true);

            Frame.Navigate(typeof(AuthPage), edit);

            if (await edit.Task)
            {
                Server server = new Server()
                {
                    SortBy = new FileSystemItemSortBy()
                    {
                        Type = FileSystemItemSortType.Name,
                        Direction = FileSystemItemSortDirection.ASC,
                    },
                    Api = newApi,
                    BackgroundOperations = viewModel.BackgroundOperations,
                };
                viewModel.Servers.Add(server);
                server.Id = await database.Servers.InsertServer(server.ToInfo());
            }
        }

        private async void AbbTest_Click(object sender, RoutedEventArgs e)
        {
            if (await ShowExceptionDialog(Settings.Current.StorageException, "Storage Exception") && 
                await ShowExceptionDialog(Settings.Current.UnhandledException, "Unhandled Exception") && 
                await ShowExceptionDialog(Settings.Current.SyncException, "Sync Exception"))
            {
                string formatedTimerSyncTime = Settings.Current.SyncTimerTime.ToString();
                await DialogUtils.ShowSafeAsync(formatedTimerSyncTime, "Timer synced");
            }
        }

        private Task<bool> ShowExceptionDialog(AppDataExceptionObject exception, string title)
        {
            string message = "<None>";
            if (exception != null)
            {
                message = $"{exception.Timestamp}\n{exception.Error}";
            }

            return DialogUtils.ShowTwoOptionsAsync(message, title, "Next", "Close");
        }
    }
}

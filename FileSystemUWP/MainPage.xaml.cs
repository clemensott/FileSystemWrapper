using FileSystemUWP.API;
using StdOttUwp;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();
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
            ((ListView)sender).SelectionChanged += LvwServers_SelectionChanged;
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
                await App.SaveViewModel();
            }
        }

        private async void IbnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            Server server = UwpUtils.GetDataContext<Server>(sender);
            bool delete = await DialogUtils.ShowTwoOptionsAsync(server.Api.Name, "Delete Server?", "Yes", "No");
            if (delete)
            {
                viewModel.Servers.Remove(server);
                await App.SaveViewModel();
            }
        }

        private async void AbbAddServer_Click(object sender, RoutedEventArgs e)
        {
            Api newApi = new Api();
            ApiEdit edit = new ApiEdit(newApi, true);

            Frame.Navigate(typeof(AuthPage), edit);

            if (await edit.Task)
            {
                Server server = new Server(viewModel.BackgroundOperations)
                {
                    Api = newApi,
                };
                viewModel.Servers.Add(server);
                await App.SaveViewModel();
            }
        }

        private async void AbbTest_Click(object sender, RoutedEventArgs e)
        {
            string exceptionText = Settings.Current.SyncExceptionText;
            string formatedExceptionTime = Settings.Current.SyncExceptionTime.ToString();

            if (string.IsNullOrWhiteSpace(exceptionText)) exceptionText = "<None>";

            await DialogUtils.ShowSafeAsync(exceptionText, formatedExceptionTime);

            string formatedTimerSyncTime = Settings.Current.SyncTimerTime.ToString();
            await DialogUtils.ShowSafeAsync(formatedTimerSyncTime, "Timer synced");
        }
    }
}

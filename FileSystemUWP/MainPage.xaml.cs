using FileSystemUWP.FileViewers;
using FileSystemUWP.Picker;
using FileSystemUWP.Sync.Definitions;
using StdOttUwp;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
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
            DataContext = viewModel = (ViewModel)e.Parameter;

            base.OnNavigatedTo(e);
        }

        private object Path_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string path = (string)value;

            return string.IsNullOrWhiteSpace(path) ? "Root" : path;
        }

        private object SymConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return UiUtils.GetSymbol((FileSystemItem)value);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool ping = await Ping(viewModel.Api);

            if (!ping) Frame.Navigate(typeof(AuthPage), viewModel.Api);

            Application.Current.LeavingBackground += Application_LeavingBackground;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.LeavingBackground -= Application_LeavingBackground;
        }

        private async void Application_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            bool ping = await Ping(viewModel.Api);

            if (!ping)
            {
                viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

                Frame.Navigate(typeof(AuthPage), viewModel.Api);
            }
            else await pcView.UpdateCurrentFolderItems();
        }

        private static async Task<bool> Ping(Api api)
        {
            return !string.IsNullOrWhiteSpace(api.BaseUrl) &&
                api.RawCookies != null && api.RawCookies.Length > 0 &&
                await api.IsAuthorized();
        }

        private async void PcView_Loaded(object sender, RoutedEventArgs e)
        {
            await pcView.SetCurrentFolder(viewModel.CurrentFolderPath);
            pcView.Type = FileSystemItemViewType.Files | FileSystemItemViewType.Folders;

            pcView.FlyoutMenuItems = new FlyoutMenuItem[] {
                CreateFlyoutMenuItem(Symbol.List, "Details", FmiDetails_Click),
                CreateFlyoutMenuItem(Symbol.Download, "Download", FmiDownload_Click),
                CreateFlyoutMenuItem(Symbol.Cancel, "Delete", FmiDelete_Click),
            };
        }

        private static FlyoutMenuItem CreateFlyoutMenuItem(Symbol? symbol, string text, EventHandler<FlyoutMenuItemClickEventArgs> clickHandler)
        {
            FlyoutMenuItem item = new FlyoutMenuItem()
            {
                Symbol = symbol,
                Text = text,
            };
            item.Click += clickHandler;

            return item;
        }

        private void FmiDetails_Click(object sender, FlyoutMenuItemClickEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

            Frame.Navigate(typeof(FileSystemItemInfoPage), (e.Item, viewModel.Api));
        }

        private async void FmiDownload_Click(object sender, FlyoutMenuItemClickEventArgs e)
        {
            if (e.Item.IsFolder)
            {
                await DialogUtils.ShowSafeAsync("Downloading a folder is not implemented");
                return;
            }

            FileSavePicker picker = new FileSavePicker()
            {
                SuggestedFileName = e.Item.Name,
                DefaultFileExtension = e.Item.Extension,
            };
            picker.FileTypeChoices.Add(e.Item.Extension, new string[] { e.Item.Extension });

            StorageFile file = await picker.PickSaveFileAsync();
            await viewModel.Api.DownlaodFile(e.Item.FullPath, file);
        }

        private async void FmiDelete_Click(object sender, FlyoutMenuItemClickEventArgs e)
        {
            if (e.Item.IsFile)
            {
                bool delete = await DialogUtils.ShowTwoOptionsAsync(e.Item.Name, "Delete File?", "Yes", "No");
                if (!delete) return;

                await viewModel.Api.DeleteFile(e.Item.FullPath);
                await pcView.UpdateFiles();
            }
            else
            {
                bool delete = await DialogUtils.ShowTwoOptionsAsync(e.Item.Name, "Delete Folder?", "Yes", "No");
                if (!delete) return;

                await viewModel.Api.DeleteFolder(e.Item.FullPath, true);
                await pcView.UpdateFolders();
            }
        }

        private async void AbbParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await pcView.SetParent();
            }
            catch { }
        }

        private async void AbbRefesh_Click(object sender, RoutedEventArgs e)
        {
            await pcView.UpdateCurrentFolderItems();
        }

        private void PcView_FileSelected(object sender, FileSystemItem e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

            FilesViewing viewing = new FilesViewing(e, pcView.GetCurrentItems(), viewModel.Api);
            Frame.Navigate(typeof(FilesPage), viewing);
        }

        private void AbbOpenSyncs_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

            Frame.Navigate(typeof(SyncPairsPage), viewModel);
        }

        private void AbbDetails_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

            FileSystemItem item = FileSystemItem.FromFolder(viewModel.CurrentFolderPath);
            Frame.Navigate(typeof(FileSystemItemInfoPage), (item, viewModel.Api));
        }

        private void AbbSettings_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolderPath;

            Frame.Navigate(typeof(AuthPage), viewModel.Api);
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

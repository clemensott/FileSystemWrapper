using FileSystemCommon;
using FileSystemUWP.FileViewers;
using FileSystemUWP.Picker;
using FileSystemUWP.Sync.Definitions;
using StdOttUwp;
using System;
using System.IO;
using System.Linq;
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
                viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

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

        private void GidThrough_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (abbDetails != null) abbDetails.IsEnabled = ((FileSystemItem?)args.NewValue).HasValue;
        }

        private async void PcView_Loaded(object sender, RoutedEventArgs e)
        {
            pcView.Type = FileSystemItemViewType.Files | FileSystemItemViewType.Folders;

            await pcView.SetCurrentFolder(viewModel.CurrentFolderPath);
        }

        private void MfiDetails_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

            FileSystemItem item = UwpUtils.GetDataContext<FileSystemItem>(sender);
            Frame.Navigate(typeof(FileSystemItemInfoPage), (item, viewModel.Api));
        }

        private async void MfiDownload_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItem item = UwpUtils.GetDataContext<FileSystemItem>(sender);

            if (item.IsFolder)
            {
                await DialogUtils.ShowSafeAsync("Downloading a folder is not implemented");
                return;
            }

            FileSavePicker picker = new FileSavePicker()
            {
                SuggestedFileName = item.Name,
                DefaultFileExtension = item.Extension,
            };
            picker.FileTypeChoices.Add(item.Extension, new string[] { item.Extension });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null) return;

            await UiUtils.TryAgain("Try again?", "Download file error", async () =>
            {
                try
                {
                    await viewModel.Api.DownloadFile(item.FullPath, file);
                    return true;
                }
                catch { }
                return false;
            }, viewModel.BackgroundOperations, "Download file...");
        }

        private async void MfiDelete_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItem item = UwpUtils.GetDataContext<FileSystemItem>(sender);

            if (item.IsFile)
            {
                bool delete = await DialogUtils.ShowTwoOptionsAsync(item.Name, "Delete File?", "Yes", "No");
                if (!delete) return;

                await viewModel.Api.DeleteFile(item.FullPath);

                await UiUtils.TryAgain("Try again?", "Delete file error",
                    () => viewModel.Api.DeleteFile(item.FullPath),
                    viewModel.BackgroundOperations, "Delete file...");
            }
            else
            {
                bool delete = await DialogUtils.ShowTwoOptionsAsync(item.Name, "Delete Folder?", "Yes", "No");
                if (!delete) return;

                await UiUtils.TryAgain("Try again?", "Delete folder error",
                    () => viewModel.Api.DeleteFolder(item.FullPath, true),
                    viewModel.BackgroundOperations, "Delete folder...");
            }

            await pcView.UpdateContent();
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
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

            FilesViewing viewing = new FilesViewing(e, pcView.GetCurrentItems().Where(i => i.IsFile), viewModel.Api);
            Frame.Navigate(typeof(FilesPage), viewing);
        }

        private void AbbOpenSyncs_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

            Frame.Navigate(typeof(SyncPairsPage), viewModel);
        }

        private void AbbDetails_Loaded(object sender, RoutedEventArgs e)
        {
            abbDetails.IsEnabled = pcView.CurrentFolder.HasValue;
        }

        private void AbbDetails_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

            Frame.Navigate(typeof(FileSystemItemInfoPage), (pcView.CurrentFolder.Value, viewModel.Api));
        }

        private void AbbSettings_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

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

        private async void AbbDeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!await DialogUtils.ShowTwoOptionsAsync("Are you sure?", "Delete current folder", "Yes", "No")) return;

            if (await UiUtils.TryAgain("Try again?", "Delete folder error",
                () => viewModel.Api.DeleteFolder(pcView.CurrentFolder.Value.FullPath, true),
                viewModel.BackgroundOperations, "Delete folder..."))
            {
                await pcView.SetParent();
            }
            else await pcView.UpdateCurrentFolderItems();
        }

        private async void AbbNewFolder_Click(object sender, RoutedEventArgs e)
        {
            string name = "New Folder";
            while (true)
            {
                TextBox tbx = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Text = name,
                };
                tbx.SelectAll();
                ContentDialogResult result = await DialogUtils.ShowContentAsync(tbx, "Name:", "Cancel", "Create");

                if (result != ContentDialogResult.Primary) break;
                name = tbx.Text.Trim();
                if (name.Length == 0 || name.Any(c => Path.GetInvalidFileNameChars().Contains(c))) continue;

                string path = Utils.JoinPaths(pcView.CurrentFolder.Value.FullPath, name);

                await UiUtils.TryAgain("Try again?", "Create folder error",
                    () => viewModel.Api.CreateFolder(path),
                    viewModel.BackgroundOperations, "Create folder...");

                await pcView.UpdateCurrentFolderItems();
                break;
            }
        }

        private async void AbbUploadFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List,
            };

            picker.FileTypeFilter.Add("*");

            StorageFile srcFile = await picker.PickSingleFileAsync();
            string path = Utils.JoinPaths(pcView.CurrentFolder.Value.FullPath, srcFile.Name);

            await UiUtils.TryAgain("Try again?", "Uplaod file error",
                () => viewModel.Api.UploadFile(path, srcFile),
                viewModel.BackgroundOperations, "Uploading file...");
            await pcView.UpdateCurrentFolderItems();
        }
    }
}

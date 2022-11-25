using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemUWP.API;
using FileSystemUWP.FileViewers;
using FileSystemUWP.Models;
using FileSystemUWP.Picker;
using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Util;
using StdOttStandard.Linq;
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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ServerExplorerPage : Page
    {
        private bool isAway = false;
        private Server viewModel;

        public ServerExplorerPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = viewModel = (Server)e.Parameter;

            base.OnNavigatedTo(e);

            if (await CheckServerConnection() && !isAway)
            {
                Application.Current.EnteredBackground += Application_EnteredBackground;
                Application.Current.LeavingBackground += Application_LeavingBackground;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;
            viewModel.RestoreFileSystemItem = pcView.GetCenteredFileSystemSortItem();

            Application.Current.EnteredBackground -= Application_EnteredBackground;
            Application.Current.LeavingBackground -= Application_LeavingBackground;
            isAway = true;

            base.OnNavigatedFrom(e);
        }

        private void Application_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;
            viewModel.RestoreFileSystemItem = pcView.GetCenteredFileSystemSortItem();
        }

        private async void Application_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            if (await CheckServerConnection())
            {
                viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;
                await pcView.UpdateCurrentFolderItems();
            }
        }

        private async Task<bool> CheckServerConnection()
        {
            try
            {
                viewModel.IsLoading = true;

                while (true)
                {
                    bool ping = await Ping(viewModel.Api);

                    if (isAway) return false;
                    if (ping) return true;

                    ContentDialogResult result = await DialogUtils.ShowContentAsync("Connecting to Server failed",
                        null, "Back", "Retry", "Change Settings");

                    if (isAway) return false;
                    if (result == ContentDialogResult.Primary) continue;
                    if (result == ContentDialogResult.Secondary) CallApiSettingsPage();
                    else Frame.GoBack();
                    return false;
                }
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }

        private static async Task<bool> Ping(Api api)
        {
            return !string.IsNullOrWhiteSpace(api.BaseUrl) &&
                await api.IsAuthorized() &&
                await api.LoadConfig();
        }

        private async void CallApiSettingsPage()
        {
            Api newApi = viewModel.Api.Clone();
            ApiEdit edit = new ApiEdit(newApi, false);
            Frame.Navigate(typeof(AuthPage), edit);

            if (await edit.Task)
            {
                viewModel.Api = newApi;
            }
        }

        private void GidThrough_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (abbDetails != null) abbDetails.IsEnabled = ((FileSystemItem?)args.NewValue).HasValue;
        }

        private async void PcView_Loaded(object sender, RoutedEventArgs e)
        {
            pcView.Type = FileSystemItemViewType.Files | FileSystemItemViewType.Folders;

            await pcView.SetCurrentFolder(viewModel.CurrentFolderPath);
            if (viewModel.RestoreFileSystemItem.HasValue)
            {
                pcView.ScrollToFileItemName(viewModel.RestoreFileSystemItem.Value);
            }
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

        private void AbbToServers_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbParent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileSystemItem? lastFolder = pcView.CurrentFolder;
                FileSystemSortItem? lastSortFolder = null;
                if (lastFolder.HasValue)
                {
                    lastSortFolder = FileSystemSortItem.FromItem(lastFolder.Value);
                }

                await pcView.SetParent();
                if (lastSortFolder.HasValue)
                {
                    pcView.ScrollToFileItemName(lastSortFolder.Value);
                }
            }
            catch { }
        }

        private async void AbbRefesh_Click(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            try
            {
                control.IsEnabled = false;

                await pcView.UpdateCurrentFolderItems();
            }
            finally
            {
                // Enabling button again does scroll for some misterious reason
                double offset = pcView.GetVerticalScrollOffset();
                control.IsEnabled = true;
                pcView.SetVerticalScrollOffset(offset);
            }
        }

        private void PcView_FileSelected(object sender, FileSystemItem e)
        {
            FilesViewing viewing = new FilesViewing(false, e, pcView.GetCurrentItems().Where(i => i.IsFile), viewModel.Api);
            Frame.Navigate(typeof(FilesPage), viewing);
            viewModel.LastFilesViewing = viewing;
        }

        private void SmallMediaPlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SmallMediaPlayerControl control = (SmallMediaPlayerControl)sender;

            control.Player = MediaPlayback.Current.Player;
            control.FileName = MediaPlayback.Current.FileName;
            if (control.Player != null)
            {
                control.Visibility = Visibility.Visible;
            }
        }

        private void SmallMediaPlayerControl_Open(object sender, EventArgs e)
        {
            FileSystemItem currentFile;
            if (!viewModel.LastFilesViewing.Files.TryFirst(f => f.Name == MediaPlayback.Current.FileName, out currentFile))
            {
                currentFile = viewModel.LastFilesViewing.CurrentFile;
            }
            FilesViewing viewing = new FilesViewing(true, currentFile,
                viewModel.LastFilesViewing.Files, viewModel.LastFilesViewing.Api);
            Frame.Navigate(typeof(FilesPage), viewing);
            viewModel.LastFilesViewing = viewing;
        }

        private void SmallMediaPlayerControl_Stop(object sender, EventArgs e)
        {
            SmallMediaPlayerControl control = (SmallMediaPlayerControl)sender;

            MediaPlayback.Current.Stop();
            control.Visibility = Visibility.Collapsed;
        }

        private void AbbOpenSyncs_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentFolderPath = pcView.CurrentFolder?.FullPath;

            Frame.Navigate(typeof(SyncPairsPage), viewModel);
        }

        private async void AbbChangeSorting_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItemSortBy? result = await FileSystemSortSelectorDialog.Start(viewModel.SortBy);
            if (result.HasValue)
            {
                viewModel.SortBy = result.Value;
            }
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

                string path = viewModel.Api.Config.JoinPaths(pcView.CurrentFolder.Value.FullPath, name);

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
            string path = viewModel.Api.Config.JoinPaths(pcView.CurrentFolder.Value.FullPath, srcFile.Name);

            await UiUtils.TryAgain("Try again?", "Uplaod file error",
                () => viewModel.Api.UploadFile(path, srcFile),
                viewModel.BackgroundOperations, "Uploading file...");
            await pcView.UpdateCurrentFolderItems();
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
    }
}

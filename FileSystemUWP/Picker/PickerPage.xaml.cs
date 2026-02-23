using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.Models;
using StdOttStandard.Linq;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.Picker
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PickerPage : Page
    {
        private FolderPicking picking;

        public PickerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            picking = (FolderPicking)e.Parameter;

            switch (picking.Type)
            {
                case FileSystemPickType.Folder:
                    pcView.Type = FileSystemItemViewType.Folders;
                    abbSelect.Icon = new SymbolIcon(Symbol.Accept);
                    abbSelect.Visibility = Visibility.Visible;
                    break;

                case FileSystemPickType.FileSave:
                    pcView.Type = FileSystemItemViewType.Folders;
                    abbSelect.Icon = new SymbolIcon(Symbol.Forward);
                    abbSelect.Visibility = Visibility.Visible;
                    break;

                case FileSystemPickType.FileOpen:
                    pcView.Type = FileSystemItemViewType.Folders | FileSystemItemViewType.Files;
                    break;
            }

            pcView.Api = picking.Api;
            await pcView.SetCurrentFolder(picking.SuggestedStartLocation);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !picking.Task.IsCompleted) picking.SetResult(null);
        }

        private void PcView_FileSelected(object sender, FileSystemItem e)
        {
            picking.SetResult(e.PathParts);
            Frame.GoBack();
        }

        private async void AbbSelect_Click(object sender, RoutedEventArgs e)
        {
            switch (picking.Type)
            {
                case FileSystemPickType.Folder:
                    picking.SetResult(pcView.CurrentFolder?.PathParts);
                    Frame.GoBack();
                    break;

                case FileSystemPickType.FileSave:
                    picking.SuggestedStartLocation = pcView.CurrentFolder?.FullPath;

                    NamePicking namePicking = NamePicking.ForFile(picking.Api,
                        pcView.CurrentFolder?.FullPath, picking.SuggestedFileName);
                    Frame.Navigate(typeof(NamePickerPage), namePicking);

                    string name = await namePicking.Task;

                    if (!namePicking.Task.IsCompleted) return;

                    PathPart[] filePathParts = (pcView.CurrentFolder?.PathParts).ToNotNull().ConcatParams(new PathPart()
                    {
                        Name = name,
                        Path = picking.Api.Config.JoinPaths(pcView.CurrentFolder?.FullPath, name),
                    }).ToArray();

                    picking.SetResult(filePathParts);
                    Frame.GoBack();
                    break;
            }
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbToParent_Click(object sender, RoutedEventArgs e)
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
    }
}

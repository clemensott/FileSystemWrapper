﻿using System.IO;
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

            await pcView.SetCurrentFolder(picking.SuggestedStartLocation);
            pcView.Api = picking.Api;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !picking.HasResult) picking.SetValue(null);
        }

        private void PcView_FileSelected(object sender, FileSystemItem e)
        {
            picking.SetValue(e.FullPath);
            Frame.GoBack();
        }

        private async void AbbSelect_Click(object sender, RoutedEventArgs e)
        {
            switch (picking.Type)
            {
                case FileSystemPickType.Folder:
                    picking.SetValue(pcView.CurrentFolderPath);
                    Frame.GoBack();
                    break;

                case FileSystemPickType.FileSave:
                    picking.SuggestedStartLocation = pcView.CurrentFolderPath;

                    NamePicking namePicking = NamePicking.ForFile(picking.Api,
                        pcView.CurrentFolderPath, picking.SuggestedFileName);
                    Frame.Navigate(typeof(NamePickerPage), namePicking);

                    string name = await namePicking.Task;

                    if (!namePicking.HasResult) return;

                    string path = Path.Combine(pcView.CurrentFolderPath, name);
                    picking.SetValue(path);
                    Frame.GoBack();
                    break;
            }
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
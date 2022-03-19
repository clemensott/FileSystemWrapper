using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.FileViewers
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FilesPage : Page
    {
        private bool isLoading, isFirstOpening, keepPlayback;
        private readonly List<FileControl> fileControls;
        private FilesViewing viewing;

        public FilesPage()
        {
            this.InitializeComponent();

            isLoading = true;
            isFirstOpening = true;
            fileControls = new List<FileControl>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            viewing = (FilesViewing)e.Parameter;

            ObservableCollection<FileSystemItem> items = new ObservableCollection<FileSystemItem>();
            items.Add(viewing.CurrentFile);

            fvwFiles.ItemsSource = items;
            fvwFiles.SelectedIndex = 0;

            foreach ((int index, FileSystemItem item) in viewing.Files.WithIndex())
            {
                if (!item.Equals(viewing.CurrentFile)) items.Insert(index, item);
            }

            isLoading = false;
            await SetCurrentContent(viewing.ResumePlayback);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!keepPlayback) MediaPlayback.Current.Stop();

            base.OnNavigatedFrom(e);
        }

        private async void FvwFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isFirstOpening) MediaPlayback.Current.Stop();
            await SetCurrentContent();
        }

        private async void FileControl_Loaded(object sender, RoutedEventArgs e)
        {
            FileControl control = (FileControl)sender;

            AddFileControl(control);

            if (isFirstOpening) await SetCurrentContent(viewing.ResumePlayback);
        }

        private void FileControl_Unloaded(object sender, RoutedEventArgs e)
        {
            fileControls.Remove((FileControl)sender);
        }

        private async void FileControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            FileControl control = (FileControl)sender;

            AddFileControl(control);

            if (isFirstOpening) await SetCurrentContent(viewing.ResumePlayback);
        }

        private void AddFileControl(FileControl control)
        {
            if (fileControls.Contains(control)) return;

            control.Api = viewing.Api;

            fileControls.Add(control);
        }

        private async Task SetCurrentContent(bool resumePlayback = false)
        {
            if (fvwFiles.SelectedItem == null || isLoading) return;

            FileSystemItem file = (FileSystemItem)fvwFiles.SelectedItem;

            foreach (FileControl control in fileControls.ToArray())
            {
                if (file.Equals(control.DataContext))
                {
                    isFirstOpening = false;
                    await control.Activate(resumePlayback);
                }
                else control.Deactivate();
            }
        }

        private void FileControl_IsFullScreenChanged(object sender, IsFullScreenChangedEventArgs e)
        {
            cbrBottom.Visibility = e.IsFullScreen ? Visibility.Collapsed : Visibility.Visible;
        }

        private void FileControl_MinimizePlayerClicked(object sender, EventArgs e)
        {
            keepPlayback = true;
            Frame.GoBack();
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbDownload_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItem item = (FileSystemItem)fvwFiles.SelectedItem;
            FileSavePicker picker = new FileSavePicker()
            {
                SuggestedFileName = item.Name,
                DefaultFileExtension = item.Extension,
            };
            picker.FileTypeChoices.Add(item.Extension, new string[] { item.Extension });

            StorageFile file = await picker.PickSaveFileAsync();
            await viewing.Api.DownloadFile(item.FullPath, file);
        }

        private void AbbDetails_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItem item = (FileSystemItem)fvwFiles.SelectedItem;
            viewing.CurrentFile = item;

            Frame.Navigate(typeof(FileSystemItemInfoPage), (item, viewing.Api));
        }
    }
}

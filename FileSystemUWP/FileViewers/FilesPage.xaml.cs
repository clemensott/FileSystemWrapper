using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
        private bool isLoading, isFirstOpening;
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
            await SetCurrentContent();

            base.OnNavigatedTo(e);
        }

        private async void FvwFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaPlayback.Current.Stop();
            await SetCurrentContent();
        }

        private async void FileControl_Loaded(object sender, RoutedEventArgs e)
        {
            FileControl control = (FileControl)sender;

            AddFileControl(control);

            if (isFirstOpening) await SetCurrentContent();
        }

        private void FileControl_Unloaded(object sender, RoutedEventArgs e)
        {
            fileControls.Remove((FileControl)sender);
        }

        private void AddFileControl(FileControl control)
        {
            if (fileControls.Contains(control)) return;

            control.Controls = new CustomViewerControls(cbrBottom, abbStop, Frame);
            control.Api = viewing.Api;

            fileControls.Add(control);
        }

        private async Task SetCurrentContent()
        {
            if (fvwFiles.SelectedItem == null || isLoading) return;

            FileSystemItem file = (FileSystemItem)fvwFiles.SelectedItem;

            foreach (FileControl control in fileControls.ToArray())
            {
                if (file.Equals(control.DataContext))
                {
                    isFirstOpening = false;
                    await control.Activate();
                }
                else control.Deactivate();
            }
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AbbStop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayback.Current.Player.Source = null;
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
            await viewing.Api.DownlaodFile(item.FullPath, file);
        }

        private void AbbDetails_Click(object sender, RoutedEventArgs e)
        {
            FileSystemItem item = (FileSystemItem)fvwFiles.SelectedItem;
            viewing.CurrentFile = item;

            Frame.Navigate(typeof(FileSystemItemInfoPage), (item, viewing.Api));
        }
    }
}

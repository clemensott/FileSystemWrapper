using FileSystemCommon.Models.FileSystem;
using StdOttUwp;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FileSystemItemInfoPage : Page
    {
        private FileSystemItem item;
        private Api api;

        public FileSystemItemInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (item, api) = ((FileSystemItem, Api))e.Parameter;
            DataContext = item;

            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshInfos();
        }

        private object VisFolderCon_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return value is FolderItemInfo ? Visibility.Visible : Visibility.Collapsed;
        }

        private object VisItemCon_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return value is FileSystemItem ? Visibility.Visible : Visibility.Collapsed;
        }

        private object SizeCon_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string[] endings = new string[] { "B", "kB", "MB", "GB", "TB", "PB", "EB" };
            double size = Convert.ToDouble((long)value);
            string ending = endings.Last();

            for (int i = 0; i < endings.Length; i++)
            {
                if (size < 1024)
                {
                    ending = endings[i];
                    break;
                }

                size = size / 1024.0;
            }

            if (size < 10) return string.Format("{0} {1}", Math.Round(size, 2), ending);
            if (size < 100) return string.Format("{0} {1}", Math.Round(size, 1), ending);

            return string.Format("{0} {1}", Math.Round(size, 0), ending);
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbRefresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshInfos();
        }

        private async Task RefreshInfos()
        {
            try
            {
                object info = item.IsFile ? (object)await api.GetFileInfo(item.FullPath) :
                    await api.GetFolderInfo(item.FullPath);

                if (info != null) DataContext = info;
                else
                {
                    await DialogUtils.ShowSafeAsync("Loading infos failed");
                    Frame.GoBack();
                }

            }
            catch (Exception exc)
            {
                await DialogUtils.ShowSafeAsync(exc.Message, "Load infos error");
                Frame.GoBack();
            }
        }
    }
}

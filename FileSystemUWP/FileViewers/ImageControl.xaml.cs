using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.FileViewers
{
    public sealed partial class ImageControl : UserControl
    {
        private readonly BitmapImage bmp;

        public ImageControl()
        {
            this.InitializeComponent();

            img.Source = bmp = new BitmapImage();
            bmp.ImageOpened += OnImageOpened;
            bmp.ImageFailed += OnImageFailed;
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            prgLoading.IsActive = false;
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            prgLoading.IsActive = false;
            tblFailMessage.Text = e.ErrorMessage;
            splFail.Visibility = Visibility.Visible;
        }

        public async Task SetSource(IRandomAccessStream stream)
        {
            await bmp.SetSourceAsync(stream);
        }

        private async void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer viewer = (ScrollViewer)sender;

            await Task.Delay(10);
            if (viewer.ZoomFactor == 1) viewer.ChangeView(null, null, 2, false);
            else viewer.ChangeView(null, null, 1, false);
        }
    }
}

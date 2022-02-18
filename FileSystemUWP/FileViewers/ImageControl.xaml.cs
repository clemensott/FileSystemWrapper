using System;
using System.Threading.Tasks;
using Windows.Foundation;
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
        private bool successfulLoaded = false;
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
            successfulLoaded = true;
            prgLoading.IsActive = false;
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            successfulLoaded = false;
            prgLoading.IsActive = false;
            tblFailMessage.Text = e.ErrorMessage;
            splFail.Visibility = Visibility.Visible;
        }

        public async Task SetSource(IRandomAccessStream stream)
        {
            successfulLoaded = false;
            await bmp.SetSourceAsync(stream);
        }

        private async void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await Task.Delay(10);

            if (FitImage())
            {
                Point point = e.GetPosition(sv);
                sv.ChangeView(point.X, point.Y, 2, false);
            }
        }

        private void Img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (successfulLoaded)
            {
                successfulLoaded = false;
                FitImage();
            }
        }

        private bool FitImage()
        {
            bool fitted = true;

            if (img.ActualWidth / sv.ViewportWidth > img.ActualHeight / sv.ViewportHeight)
            {
                if (img.ActualWidth != sv.ViewportWidth && img.Width != sv.ViewportWidth)
                {
                    img.Width = sv.ViewportWidth;
                    img.Height = double.NaN;
                    fitted = false;
                }
            }
            else if (img.ActualHeight != sv.ViewportHeight && img.Height != sv.ViewportHeight)
            {
                img.Width = double.NaN;
                img.Height = sv.ViewportHeight;
                fitted = false;
            }
            if (sv.ZoomFactor != 1)
            {
                sv.ChangeView(null, null, 1, false);
                fitted = false;
            }

            return fitted;
        }
    }
}

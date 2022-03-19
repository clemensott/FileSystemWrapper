using System;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.FileViewers
{
    public sealed partial class SmallMediaPlayerControl : UserControl
    {
        private object lastTappedArgs = null;

        public event EventHandler Open;
        public event EventHandler Stop;

        public static readonly DependencyProperty MaxSpaceUsageProperty =
            DependencyProperty.Register(nameof(MaxSpaceUsage), typeof(double), typeof(SmallMediaPlayerControl),
                new PropertyMetadata(default(double), OnMaxSpaceUsagePropertyChanged));

        private static void OnMaxSpaceUsagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SmallMediaPlayerControl s = (SmallMediaPlayerControl)sender;

            s.SetSize();
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(nameof(FileName), typeof(string), typeof(SmallMediaPlayerControl),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register(nameof(Player), typeof(MediaPlayer), typeof(SmallMediaPlayerControl),
                new PropertyMetadata(default(MediaPlayer), OnPlayerPropertyChanged));

        private static void OnPlayerPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SmallMediaPlayerControl s = (SmallMediaPlayerControl)sender;
            MediaPlayer oldValue = (MediaPlayer)e.OldValue;
            MediaPlayer newValue = (MediaPlayer)e.NewValue;

            s.Unsubscribe(oldValue);
            s.mpe.SetMediaPlayer(newValue);
            s.Subscribe(newValue);
            s.SetMediaMode();
            s.SetSize();
        }

        public double MaxSpaceUsage
        {
            get => (double)GetValue(MaxSpaceUsageProperty);
            set => SetValue(MaxSpaceUsageProperty, value);
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public MediaPlayer Player
        {
            get => (MediaPlayer)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public SmallMediaPlayerControl()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSize();
        }

        private void Main_Tapped(object sender, object e)
        {
            if (Equals(e, lastTappedArgs)) lastTappedArgs = null;
            else Open?.Invoke(this, EventArgs.Empty);
        }

        private void Emtc_Tapped(object sender, object e)
        {
            lastTappedArgs = e;
        }

        private void Mpe_Unloaded(object sender, RoutedEventArgs e)
        {
            Unsubscribe(Player);
        }

        private void Emtc_StopClicked(object sender, EventArgs e)
        {
            Stop?.Invoke(this, EventArgs.Empty);
        }

        private void Subscribe(MediaPlayer player)
        {
            if (player == null) return;

            player.MediaFailed += Player_MediaFailed;
            player.PlaybackSession.NaturalVideoSizeChanged += PlaybackSession_NaturalVideoSizeChanged;
        }

        private void Unsubscribe(MediaPlayer player)
        {
            if (player == null) return;

            player.MediaFailed -= Player_MediaFailed;
            player.PlaybackSession.NaturalVideoSizeChanged -= PlaybackSession_NaturalVideoSizeChanged;
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Stop?.Invoke(this, EventArgs.Empty);
        }

        private async void PlaybackSession_NaturalVideoSizeChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, SetMediaMode);
        }

        private void SetMediaMode()
        {
            if (Player == null) return;

            if (Player.PlaybackSession.NaturalVideoHeight > 0)
            {
                main.VerticalAlignment = VerticalAlignment.Bottom;
                main.HorizontalAlignment = HorizontalAlignment.Right;
                mpe.SetValue(Grid.RowProperty, 0);
                mpe.SetValue(Grid.RowSpanProperty, 2);
                rdnPlayer.Height = new GridLength(1, GridUnitType.Star);
                gidFileNameBackground.Visibility = Visibility.Visible;
            }
            else
            {
                main.Width = double.NaN;
                main.Height = double.NaN;
                main.VerticalAlignment = VerticalAlignment.Bottom;
                main.HorizontalAlignment = HorizontalAlignment.Stretch;
                mpe.SetValue(Grid.RowProperty, 1);
                mpe.SetValue(Grid.RowSpanProperty, 1);
                rdnPlayer.Height = GridLength.Auto;
                gidFileNameBackground.Visibility = Visibility.Collapsed;
            }
        }

        private void SetSize()
        {
            if (Player != null && Player.PlaybackSession.NaturalVideoHeight > 0) SetVideoSize();
        }

        private void SetVideoSize()
        {
            Size maxRatioSize = GetMaxRatioSize();

            double factor = GetUsageFactorOfMaxRatioSize(maxRatioSize);
            double rootFactor = factor < 1 ? Math.Sqrt(factor) : 1;

            main.Width = maxRatioSize.Width * rootFactor;
            main.Height = maxRatioSize.Height * rootFactor;
        }

        private Size GetMaxRatioSize()
        {
            double videoRatio = Player.PlaybackSession.NaturalVideoWidth / (double)Player.PlaybackSession.NaturalVideoHeight;
            double parentRatio = ActualWidth / ActualHeight;

            return videoRatio < parentRatio ?
                new Size(ActualHeight * videoRatio, ActualHeight) :
                new Size(ActualWidth, ActualWidth / videoRatio);
        }

        private double GetUsageFactorOfMaxRatioSize(Size maxRatioSize)
        {
            double totalSpace = ActualWidth * ActualHeight;
            double maxRatioSpace = maxRatioSize.Width * maxRatioSize.Height;
            return MaxSpaceUsage * totalSpace / maxRatioSpace;
        }
    }
}

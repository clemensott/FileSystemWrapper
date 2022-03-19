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

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(nameof(FileName), typeof(string), typeof(SmallMediaPlayerControl),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ParentWidthProperty =
            DependencyProperty.Register(nameof(ParentSize), typeof(Size), typeof(SmallMediaPlayerControl),
                new PropertyMetadata(Size.Empty, OnParentSizePropertyChanged));

        private static void OnParentSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SmallMediaPlayerControl s = (SmallMediaPlayerControl)sender;

            s.SetSize();
        }

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
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public Size ParentSize
        {
            get => (Size)GetValue(ParentWidthProperty);
            set => SetValue(ParentWidthProperty, value);
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
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, SetSize);
        }

        private void SetSize()
        {
            if (Player != null)
            {
                if (Player.PlaybackSession.NaturalVideoHeight > 0)
                {
                    double videoRatio = Player.PlaybackSession.NaturalVideoWidth / (double)Player.PlaybackSession.NaturalVideoHeight;
                    double parentRatio = ActualWidth / ActualHeight;

                    if (videoRatio < parentRatio)
                    {
                        main.Width = ActualHeight * videoRatio;
                        main.Height = ActualHeight;
                    }
                    else if (videoRatio > parentRatio)
                    {
                        main.Width = ActualWidth;
                        main.Height = ActualWidth / videoRatio;
                    }

                    mpe.SetValue(Grid.RowProperty, 0);
                    mpe.SetValue(Grid.RowSpanProperty, 2);
                    rdnPlayer.Height = new GridLength(1, GridUnitType.Star);
                    gidFileNameBackground.Visibility = Visibility.Visible;
                }
                else
                {
                    VerticalAlignment = VerticalAlignment.Bottom;
                    HorizontalAlignment = HorizontalAlignment.Right;
                    main.MaxWidth = double.PositiveInfinity;
                    main.MaxHeight = double.PositiveInfinity;
                    mpe.SetValue(Grid.RowProperty, 1);
                    mpe.SetValue(Grid.RowSpanProperty, 1);
                    rdnPlayer.Height = GridLength.Auto;
                    gidFileNameBackground.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}

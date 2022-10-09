using System;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.FileViewers
{
    public sealed partial class MediaControl : UserControl
    {
        private readonly EventifiedMediaTransportControls mtc;

        public event EventHandler<IsFullScreenChangedEventArgs> IsFullScreenChanged;
        public event EventHandler MinimizePlayerClicked;

        public MediaControl(MediaPlayer player)
        {
            this.InitializeComponent();

            mtc = (EventifiedMediaTransportControls)mpe.TransportControls;
            SetPlayer(player);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SetPlayer(null);
            SetIsFullScreen(false);
        }

        private void SetPlayer(MediaPlayer player)
        {
            Unsubscribe(mpe.MediaPlayer);
            mpe.SetMediaPlayer(player);

            if (player == null) return;

            Subscribe(player);

            MediaPlaybackState state = player.PlaybackSession.PlaybackState;
            prgLoading.IsActive = state != MediaPlaybackState.Paused && state != MediaPlaybackState.Playing;

            SystemMediaTransportControls smtc = player.SystemMediaTransportControls;
            smtc.IsPreviousEnabled =
                smtc.IsChannelDownEnabled =
                smtc.IsChannelUpEnabled =
                smtc.IsFastForwardEnabled =
                smtc.IsRecordEnabled =
                smtc.IsRewindEnabled = false;
            smtc.IsEnabled =
                smtc.IsPauseEnabled =
                smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = smtc.IsPreviousEnabled = false;
        }

        private void Subscribe(MediaPlayer player)
        {
            if (player == null) return;

            player.MediaOpened += Player_MediaOpened;
            player.MediaFailed += Player_MediaFailed;
            player.BufferingStarted += Player_BufferingStarted;
            player.BufferingEnded += Player_BufferingEnded;
        }

        private void Unsubscribe(MediaPlayer player)
        {
            if (player == null) return;

            player.MediaOpened -= Player_MediaOpened;
            player.MediaFailed -= Player_MediaFailed;
            player.BufferingStarted -= Player_BufferingStarted;
            player.BufferingEnded -= Player_BufferingEnded;
        }

        private async void Player_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => prgLoading.IsActive = false);
        }

        private async void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => tblError.Text = args.ErrorMessage);
        }

        private async void Player_BufferingStarted(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => prgLoading.IsActive = true);
        }

        private async void Player_BufferingEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => prgLoading.IsActive = false);
        }

        private void Mpe_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (mpe.MediaPlayer.PlaybackSession.NaturalVideoHeight > 0)
            {
                SetIsFullScreen(!mpe.IsFullWindow);
            }
        }

        private void SetIsFullScreen(bool isFullWindow)
        {
            mpe.IsFullWindow = isFullWindow;
            UpdateIsFullScreen(isFullWindow);
        }

        private void UpdateIsFullScreen(bool isFullWindow)
        {
            mtc.IsBackToWindowButtonVisable = !isFullWindow;
            IsFullScreenChanged?.Invoke(this, new IsFullScreenChangedEventArgs(isFullWindow));
        }

        private void Emtc_BackToWindowClicked(object sender, EventArgs e)
        {
            MinimizePlayerClicked?.Invoke(this, EventArgs.Empty);
            SetIsFullScreen(false);
        }

        private void Emtc_FullWindowClicked(object sender, EventArgs e)
        {
            UpdateIsFullScreen(!mpe.IsFullWindow);
        }
    }
}

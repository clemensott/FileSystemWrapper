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
        private readonly CustomViewerControls controls;

        public MediaControl(MediaPlayer player, CustomViewerControls controls)
        {
            this.InitializeComponent();

            this.controls = controls;
            controls.AbbStop.Visibility = Visibility.Visible;

            SetPlayer(player);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            mpe.TransportControls.IsEnabled =
                mpe.TransportControls.IsSeekBarVisible =
                mpe.TransportControls.IsCompact =
                mpe.TransportControls.IsSeekEnabled = true;

            mpe.TransportControls.IsFastForwardButtonVisible =
                mpe.TransportControls.IsFastRewindButtonVisible =
                mpe.TransportControls.IsFullWindowButtonVisible =
                mpe.TransportControls.IsPlaybackRateButtonVisible =
                mpe.TransportControls.IsSkipBackwardButtonVisible =
                mpe.TransportControls.IsSkipForwardButtonVisible =
                mpe.TransportControls.IsStopButtonVisible =
                mpe.TransportControls.IsVolumeButtonVisible =
                mpe.TransportControls.IsZoomButtonVisible =
                mpe.TransportControls.IsNextTrackButtonVisible =
                mpe.TransportControls.IsPreviousTrackButtonVisible = false;

            mpe.TransportControls.Visibility = Visibility.Visible;
            mpe.AreTransportControlsEnabled = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SetPlayer(null);
        }

        private void SetPlayer(MediaPlayer player)
        {
            Unsubscribe(mpe.MediaPlayer);
            mpe.SetMediaPlayer(player);

            if (player == null) return;

            Subscribe(player);

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
            if (mpe.IsFullWindow || mpe.MediaPlayer.PlaybackSession.NaturalVideoHeight == 0)
            {
                mpe.IsFullWindow = false;
                controls.BottomAppBar.Visibility = Visibility.Visible;
            }
            else
            {
                mpe.IsFullWindow = true;
                controls.BottomAppBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}

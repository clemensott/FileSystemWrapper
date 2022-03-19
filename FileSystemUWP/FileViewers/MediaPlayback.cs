using System.ComponentModel;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace FileSystemUWP.FileViewers
{
    class MediaPlayback : INotifyPropertyChanged
    {
        private static MediaPlayback instance;

        public static MediaPlayback Current
        {
            get
            {
                if (instance == null) instance = new MediaPlayback();

                return instance;
            }
        }

        private string fileName, contenType;
        private MediaPlayer player;
        private IRandomAccessStream stream;

        public string FileName
        {
            get => fileName;
            private set
            {
                if (value == fileName) return;

                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string ContenType
        {
            get => contenType;
            private set
            {
                if (value == contenType) return;

                contenType = value;
                OnPropertyChanged(nameof(ContenType));
            }
        }

        public MediaPlayer Player
        {
            get => player;
            private set
            {
                if (value == player) return;

                player = value;
                OnPropertyChanged(nameof(Player));
            }
        }

        public IRandomAccessStream Stream
        {
            get => stream;
            private set
            {
                if (value == stream) return;

                stream = value;
                OnPropertyChanged(nameof(Stream));
            }
        }

        private MediaPlayback()
        {
        }

        public void Stop()
        {
            MediaPlayer oldPlayer = Player;
            IRandomAccessStream oldStream = Stream;

            Player = null;
            Stream = null;
            FileName = null;
            ContenType = null;

            if (oldPlayer != null) oldPlayer.Source = null;
            oldStream?.Dispose();
        }

        public MediaPlayer SetSource(IRandomAccessStream stream, string fileName, string contenType)
        {
            MediaPlayer oldPlayer = Player;
            IRandomAccessStream oldStream = Stream;

            Player = new MediaPlayer();
            Stream = stream;
            FileName = fileName;
            ContenType = contenType;

            Player.Source = MediaSource.CreateFromStream(stream, contenType);
            Player.Play();

            if (oldPlayer != null) oldPlayer.Source = null;
            oldStream?.Dispose();

            SystemMediaTransportControls smtc = Player.SystemMediaTransportControls;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = fileName;
            smtc.DisplayUpdater.Update();
            return Player;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

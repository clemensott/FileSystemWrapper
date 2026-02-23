using FileSystemUWP.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileSystemUWP.Models
{
    class ViewModel : INotifyPropertyChanged
    {
        private bool isLoaded;
        private Server currentServer;

        public bool IsLoaded
        {
            get => isLoaded;
            set
            {
                if (value == isLoaded) return;

                isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        public Server CurrentServer
        {
            get => currentServer;
            set
            {
                if (value == currentServer) return;

                currentServer = value;
                OnPropertyChanged(nameof(CurrentServer));
            }
        }

        public ObservableCollection<Server> Servers { get; }

        public BackgroundOperations BackgroundOperations { get; }

        public ViewModel()
        {
            Servers = new ObservableCollection<Server>();
            BackgroundOperations = new BackgroundOperations();
        }

        public void InjectData(IEnumerable<Server> servers, int? currentServerId)
        {
            Servers.Clear();
            foreach (Server server in servers)
            {
                server.BackgroundOperations = BackgroundOperations;
                Servers.Add(server);

                if (server.Id == currentServerId) CurrentServer = server;
            }

            IsLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

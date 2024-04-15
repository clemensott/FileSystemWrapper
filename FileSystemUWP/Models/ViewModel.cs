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

        public ViewModel(IEnumerable<Server> servers, int? currentServerId)
        {
            Servers = new ObservableCollection<Server>(servers);
            BackgroundOperations = new BackgroundOperations();

            foreach (Server server in Servers)
            {
                server.BackgroundOperations = BackgroundOperations;

                if (server.Id == currentServerId) CurrentServer = server;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

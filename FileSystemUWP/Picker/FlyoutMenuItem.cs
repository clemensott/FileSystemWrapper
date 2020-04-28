using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace FileSystemUWP.Picker
{
    public class FlyoutMenuItem
    {
        public event EventHandler<FlyoutMenuItemClickEventArgs> Click;

        public Symbol? Symbol { get; set; }

        public string Text { get; set; }

        public void Raise(FileSystemItem item)
        {
            Click?.Invoke(this, new FlyoutMenuItemClickEventArgs(item));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

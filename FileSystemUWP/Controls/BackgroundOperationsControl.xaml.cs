using StdOttStandard.Linq;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.Controls
{
    public sealed partial class BackgroundOperationsControl : UserControl
    {

        public static readonly DependencyProperty OperationsProperty =
            DependencyProperty.Register(nameof(Operations), typeof(BackgroundOperations), typeof(BackgroundOperationsControl),
                new PropertyMetadata(default(BackgroundOperations), OnOperationsPropertyChanged));

        private static void OnOperationsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BackgroundOperationsControl s = (BackgroundOperationsControl)sender;
            BackgroundOperations oldValue = (BackgroundOperations)e.OldValue;
            BackgroundOperations newValue = (BackgroundOperations)e.NewValue;

            if (oldValue != null) oldValue.CollectionChanged -= s.Operations_CollectionChanged;
            if (newValue != null) newValue.CollectionChanged += s.Operations_CollectionChanged;

            s.UpdateText();
        }

        public BackgroundOperations Operations
        {
            get => (BackgroundOperations)GetValue(OperationsProperty);
            set => SetValue(OperationsProperty, value);
        }

        public BackgroundOperationsControl()
        {
            this.InitializeComponent();

            UpdateText();
        }

        private void Operations_CollectionChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            tbxNames.Text = Operations.ToNotNull().Select(p => p.Value).Where(l => !string.IsNullOrWhiteSpace(l)).Join();
            gidMain.Visibility = Operations == null || Operations.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

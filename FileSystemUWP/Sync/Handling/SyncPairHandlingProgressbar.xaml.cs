using FileSystemCommonUWP.Sync.Handling;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.Sync.Handling
{
    public sealed partial class SyncPairHandlingProgressbar : UserControl
    {
        private SyncPairRun run;

        public SyncPairHandlingProgressbar()
        {
            this.InitializeComponent();

            SetProgress();
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (run != null) run.PropertyChanged -= Response_PropertyChanged;
            run = (SyncPairRun)args.NewValue;
            if (run != null) run.PropertyChanged += Response_PropertyChanged;

            SetProgress();
        }

        private void Response_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SyncPairRun.State):
                case nameof(SyncPairRun.CurrentCount):
                case nameof(SyncPairRun.AllFilesCount):
                    SetProgress();
                    break;
            }
        }

        private void SetProgress()
        {
            bool isWaiting;
            if (run == null) isWaiting = true;
            else
            {
                isWaiting = run.State == SyncPairHandlerState.Loading ||
                  run.State == SyncPairHandlerState.WaitForStart ||
                  (run.State == SyncPairHandlerState.Running && run.AllFilesCount == 0);
            }

            if (isWaiting)
            {
                tblCurrent.Visibility = Visibility.Collapsed;
                tblTotal.Visibility = Visibility.Collapsed;
                pgbProgress.IsIndeterminate = true;
            }
            else
            {
                tblCurrent.Text = run.CurrentCount.ToString();
                tblTotal.Text = run.AllFilesCount.ToString();
                pgbProgress.Value = run.CurrentCount;
                pgbProgress.Maximum = run.AllFilesCount;

                tblCurrent.Visibility = Visibility.Visible;
                tblTotal.Visibility = Visibility.Visible;
                pgbProgress.IsIndeterminate = false;
            }
        }
    }
}

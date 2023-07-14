using FileSystemCommonUWP.Sync.Handling;
using FileSystemCommonUWP.Sync.Handling.Communication;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.Sync.Handling
{
    public sealed partial class SyncPairHandlingProgressbar : UserControl
    {
        private SyncPairResponseInfo response;

        public SyncPairHandlingProgressbar()
        {
            this.InitializeComponent();

            SetProgress();
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (response != null) response.PropertyChanged -= Response_PropertyChanged;
            response = (SyncPairResponseInfo)args.NewValue;
            if (response != null) response.PropertyChanged += Response_PropertyChanged;

            SetProgress();
        }

        private void Response_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SyncPairResponseInfo.State):
                case nameof(SyncPairResponseInfo.CurrentCount):
                case nameof(SyncPairResponseInfo.TotalCount):
                    SetProgress();
                    break;
            }
        }

        private void SetProgress()
        {
            bool isWaiting;
            if (response == null) isWaiting = true;
            else
            {
                isWaiting = response.State == SyncPairHandlerState.Loading ||
                  response.State == SyncPairHandlerState.WaitForStart ||
                  (response.State == SyncPairHandlerState.Running && response.TotalCount == 0);
            }

            if (isWaiting)
            {
                tblCurrent.Visibility = Visibility.Collapsed;
                tblTotal.Visibility = Visibility.Collapsed;
                pgbProgress.IsIndeterminate = true;
            }
            else
            {
                tblCurrent.Text = response.CurrentCount.ToString();
                tblTotal.Text = response.TotalCount.ToString();
                pgbProgress.Value = response.CurrentCount;
                pgbProgress.Maximum = response.TotalCount;

                tblCurrent.Visibility = Visibility.Visible;
                tblTotal.Visibility = Visibility.Visible;
                pgbProgress.IsIndeterminate = false;
            }
        }
    }
}

using FileSystemCommon;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.Picker
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NamePickerPage : Page
    {
        private const Symbol errorSymbol = Symbol.Cancel;

        private NamePicking picking;

        public NamePickerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            picking = (NamePicking)e.Parameter;

            if (!string.IsNullOrWhiteSpace(picking.Suggestion)) tbxName.Text = picking.Suggestion;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !picking.Task.IsCompleted) picking.SetResult(null);
        }

        private async void TbxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = tbxName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                abbAccept.IsEnabled = false;
                return;
            }

            if (picking.FileConfilctType == ConflictType.Ignore &&
                picking.FolderConflictType == ConflictType.Ignore) return;

            sinStatus.Visibility = Visibility.Collapsed;
            prgLoading.IsActive = true;
            abbAccept.IsEnabled = false;

            string path = picking.Api.Config.JoinPaths(picking.FolderPath, name);
            Task<bool> fileExistsTask = picking.Api.FileExists(path);
            Task<bool> folderExistsTask = picking.Api.FolderExists(path);
            bool fileExists = await fileExistsTask;
            bool folderExists = await folderExistsTask;

            if (name != tbxName.Text) return;

            if (fileExists) sinStatus.Symbol = GetSymbol(picking.FileConfilctType);
            else if (folderExists) sinStatus.Symbol = GetSymbol(picking.FolderConflictType);
            else sinStatus.Symbol = Symbol.Accept;

            prgLoading.IsActive = false;
            sinStatus.Visibility = Visibility.Visible;
            abbAccept.IsEnabled = sinStatus.Symbol != errorSymbol;
        }

        private static Symbol GetSymbol(ConflictType type)
        {
            switch (type)
            {
                case ConflictType.Ignore:
                    return Symbol.Accept;

                case ConflictType.Error:
                    return errorSymbol;

                case ConflictType.Warning:
                    return Symbol.ReportHacked;
            }

            return Symbol.Emoji;
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void AbbAccept_Click(object sender, RoutedEventArgs e)
        {
            picking.SetResult(tbxName.Text);
        }
    }
}

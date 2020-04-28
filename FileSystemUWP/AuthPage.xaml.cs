using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AuthPage : Page
    {
        private Api api;

        public AuthPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            api = (Api)e.Parameter;
            tbxBaseUrl.Text = api.BaseUrl ?? "https://";
            pbxPassword.Password = api.Password ?? string.Empty;

            base.OnNavigatedTo(e);
        }

        private void AbbApply_Click(object sender, RoutedEventArgs e)
        {
            api.BaseUrl = tbxBaseUrl.Text;
            api.Password = pbxPassword.Password;

            Frame.GoBack();
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

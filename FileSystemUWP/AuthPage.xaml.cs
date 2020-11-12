using FileSystemCommon.Models.Auth;
using System.Threading.Tasks;
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            api = (Api)e.Parameter;
            tbxBaseUrl.Text = api.BaseUrl ?? "https://";

            await UpdateBaseUrlStatus();
        }

        private async void TbxBaseUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            await UpdateBaseUrlStatus();
        }

        private async Task<bool> UpdateBaseUrlStatus()
        {
            api.BaseUrl = tbxBaseUrl.Text;
            sinBaseUrlStatus.Symbol = Symbol.Sync;
            bool successful = await api.Ping();
            sinBaseUrlStatus.Symbol = successful ? Symbol.Accept : Symbol.Dislike;

            return successful;
        }

        private async void AbbApply_Click(object sender, RoutedEventArgs e)
        {
            Control element = (Control)sender;

            try
            {
                element.IsEnabled = false;

                if (!await UpdateBaseUrlStatus())
                {
                    tblError.Text = "Cannot connect to server";
                    tblError.Visibility = Visibility.Visible;
                    return;
                }

                LoginBody body = new LoginBody()
                {
                    Username = tbxUsername.Text,
                    Password = pbxPassword.Password,
                    KeepLoggedIn = true,
                };

                if (await api.Login(body))
                {
                    Frame.GoBack();
                    return;
                }

                tblError.Text = "Please enter a correct Username and password";
                tblError.Visibility = Visibility.Visible;
            }
            finally
            {
                element.IsEnabled = true;
            }
            //Frame.GoBack();
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

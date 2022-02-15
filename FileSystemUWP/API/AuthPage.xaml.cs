using FileSystemCommon.Models.Auth;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.API
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AuthPage : Page
    {
        private bool changedLoginData;
        private ApiEdit edit;

        public AuthPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            edit = (ApiEdit)e.Parameter;
            tblTitlePrefix.Text = edit.IsAdd ? "Add" : "Edit";
            tbxServerName.Text = edit.Api.Name ?? string.Empty;
            tbxBaseUrl.Text = edit.Api.BaseUrl ?? "https://";
            tbxUsername.Text = edit.Api.Username ?? string.Empty;
            changedLoginData = false;

            await UpdateBaseUrlStatus();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !edit.Task.IsCompleted) edit.SetResult(false);

            base.OnNavigatedFrom(e);
        }

        private async void TbxBaseUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            await UpdateBaseUrlStatus();
        }

        private async Task<bool> UpdateBaseUrlStatus()
        {
            edit.Api.BaseUrl = tbxBaseUrl.Text;
            sinBaseUrlStatus.Symbol = Symbol.Sync;
            bool successful = await edit.Api.Ping();
            sinBaseUrlStatus.Symbol = successful ? Symbol.Accept : Symbol.Dislike;

            return successful;
        }

        private void TbxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            changedLoginData = true;
        }

        private void PbxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            changedLoginData = true;
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

                if (changedLoginData)
                {
                    LoginBody body = new LoginBody()
                    {
                        Username = tbxUsername.Text,
                        Password = pbxPassword.Password,
                        KeepLoggedIn = true,
                    };

                    if (await edit.Api.Login(body))
                    {
                        edit.Api.Name = tbxServerName.Text;
                        edit.Api.Username = body.Username;

                        GoBack(true);
                        return;
                    }
                }
                else if (await edit.Api.IsAuthorized())
                {
                    edit.Api.Name = tbxServerName.Text;

                    GoBack(true);
                    return;

                }

                tblError.Text = "Please enter a correct Username and password";
                tblError.Visibility = Visibility.Visible;
            }
            finally
            {
                element.IsEnabled = true;
            }
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            GoBack(false);
        }

        private void GoBack(bool success)
        {
            edit.SetResult(success);
            Frame.GoBack();
        }
    }
}

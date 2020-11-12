namespace FileSystemCommon.Models.Auth
{
    public class LoginBody
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public bool KeepLoggedIn { get; set; }
    }
}

namespace FileSystemCommon.Models.Users
{
    public class AddUserBody
    {
        public string UserName { get; set; }

        public string Password { get; set; }
        
        public string[] RoleIds { get; set; }
    }
}

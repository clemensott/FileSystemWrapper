namespace FileSystemCommon.Models.Users
{
    public class ChangePasswordBody
    {
        public  string OldPassword { get; set; }
        
        public string NewPassword { get; set; }
    }
}
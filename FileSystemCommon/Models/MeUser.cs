namespace FileSystemCommon.Models
{
    public class MeUser
    {
        public string Id { get; set; }

        public string Name { get; set; }
        
        public string[] Roles { get; set; }
        
        public string[] Permissions { get; set; }
    }
}
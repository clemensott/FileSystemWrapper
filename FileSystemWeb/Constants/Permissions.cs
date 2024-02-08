namespace FileSystemWeb.Constants
{
    class Permissions
    {
        public class Share
        {
            public const string GetShareFiles = "permissions.share.get_share_files";
            public const string PostShareFile = "permissions.share.post_share_file";
            public const string GetShareFile = "permissions.share.get_share_file";
            public const string PutShareFile = "permissions.share.get_share_file";
            public const string DeleteShareFile = "permissions.share.delete_share_file";
            public const string GetShareFolders = "permissions.share.get_share_folders";
            public const string PostShareFolder = "permissions.share.post_share_folder";
            public const string GetShareFolder = "permissions.share.get_share_folder";
            public const string PutShareFolder = "permissions.share.get_share_folder";
            public const string DeleteShareFolder = "permissions.share.delete_share_folder";
        }

        public class Users
        {
            public const string GetAllUsers = "permissions.users.get_all_users";
            public const string PostUser = "permissions.users.post_user";
            public const string DeleteUser = "permissions.users.post_user";
        }
    }
}

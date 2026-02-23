using System;

namespace FileSystemCommon.Models.Share
{
    public class EditFileSystemItemShareBody
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public bool IsListed { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
}

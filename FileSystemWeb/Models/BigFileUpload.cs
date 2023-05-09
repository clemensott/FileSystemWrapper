using System;

namespace FileSystemWeb.Models
{
    public class BigFileUpload
    {
#nullable enable
        public int Id { get; set; }

        public Guid Uuid { get; set; } = Guid.NewGuid();

        public string DestinationPath { get; set; } = string.Empty;
        
        public string TempPath { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public DateTime LastActivity { get; set; }
#nullable disable
    }
}

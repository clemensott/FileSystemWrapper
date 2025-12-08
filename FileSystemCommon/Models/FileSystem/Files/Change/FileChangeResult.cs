using System;

namespace FileSystemCommon.Models.FileSystem.Files.Change
{
    public class FileChangeResult
    {
        public FileChangeInfo[] Changes { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public bool HasMore { get; set; }
        
        public DateTime? NextTimestamp { get; set; }
    }
}
using System;

namespace FileSystemCommon.Models.FileSystem.Folders.Change
{
    public class FolderChangeResult
    {
        public FolderChangeInfo[] Changes { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public bool HasMore { get; set; }
        
        public DateTime? NextTimestamp { get; set; }
    }
}
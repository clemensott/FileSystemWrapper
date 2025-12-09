using System;

namespace FileSystemCommon.Models.FileSystem.Folders.Change
{
    public struct FolderChangeInfo
    {
        public string Path { get; set; }
        
        public string RelativePath { get; set; }
        
        public FolderChangeType ChangeType { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}
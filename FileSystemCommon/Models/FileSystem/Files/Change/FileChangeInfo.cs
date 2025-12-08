using System;

namespace FileSystemCommon.Models.FileSystem.Files.Change
{
    public struct FileChangeInfo
    {
        public string Path { get; set; }
        
        public string RelativePath { get; set; }
        
        public FileChangeType ChangeType { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}
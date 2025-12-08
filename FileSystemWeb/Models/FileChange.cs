using System;

namespace FileSystemWeb.Models;

public class FileChange
{
    public int Id { get; set; }
    
    public string Path { get; set; }
    
    public FileChangeType ChangeType { get; set; }
    
    public DateTime Timestamp { get; set; }
}
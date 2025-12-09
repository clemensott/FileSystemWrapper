using System;
using FileSystemCommon.Models.FileSystem.Files.Change;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Models;

[Index(nameof(Path), IsUnique = true)]
[Index(nameof(Timestamp))]
public class FileChange
{
    public int Id { get; set; }
    
    public string Path { get; set; }
    
    public FileChangeType ChangeType { get; set; }
    
    public DateTime Timestamp { get; set; }
}
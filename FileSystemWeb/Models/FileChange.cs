using System;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Models;

[Index(nameof(Path))]
public class FileChange
{
    public int Id { get; set; }
    
    public string Path { get; set; }
    
    public FileChangeType ChangeType { get; set; }
    
    public DateTime Timestamp { get; set; }
}
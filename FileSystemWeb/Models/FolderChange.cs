using System;
using FileSystemCommon.Models.FileSystem.Folders;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Models;

[Index(nameof(Path), IsUnique = true)]
[Index(nameof(Timestamp))]
public class FolderChange
{
    public int Id { get; set; }
    
    public string Path { get; set; }
    
    public FolderChangeType ChangeType { get; set; }
    
    public DateTime Timestamp { get; set; }
}
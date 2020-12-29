using System;

namespace FileSystemCommon.Models.FileSystem
{
    public interface IPathItem
    {
        string Name { get; }

        string Path { get; }
        
        Guid? SharedId { get; set; }
    }
}

using System;

namespace FileSystemCommon.Models.FileSystem
{
    public struct PathPart: IEquatable<PathPart>
    {
        public string Name { get; set; }

        // Virtual Path
        public string Path { get; set; }

        public bool Equals(PathPart other)
        {
            return Name == other.Name
                && Path == other.Path;
        }
    }
}

namespace FileSystemCommon.Models.FileSystem.Files.Change   
{
    public enum FileChangeType
    {
        Deleted,
        Written,
        MovedFrom,
        MovedTo,
        CopiedTo,
    }
}
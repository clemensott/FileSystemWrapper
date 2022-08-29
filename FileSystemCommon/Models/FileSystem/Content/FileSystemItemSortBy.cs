namespace FileSystemCommon.Models.FileSystem.Content
{
    public struct FileSystemItemSortBy
    {
        public FileSystemItemSortType Type { get; set; }

        public FileSystemItemSortDirection Direction { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FileSystemItemSortBy by &&
                   Type == by.Type &&
                   Direction == by.Direction;
        }

        public override int GetHashCode()
        {
            int hashCode = -601715073;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            return hashCode;
        }
    }
}

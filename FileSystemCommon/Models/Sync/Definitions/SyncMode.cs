namespace FileSystemCommon.Models.Sync.Definitions
{
    public enum SyncMode
    {
        ServerToLocalCreateOnly,
        ServerToLocal,
        LocalToServerCreateOnly,
        LocalToServer,
        TwoWay,
    }
}

using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling;
using StdOttStandard.Linq;

namespace FileSystemUWP.Sync
{
    static class SyncPairExtensions
    {
        public static SyncPair Update(this SyncPair dest, SyncPair src)
        {
            if (dest == null || src == null) return src;

            dest.Id = src.Id;
            dest.WithSubfolders = src.WithSubfolders;
            dest.CurrentSyncPairRunId = src.CurrentSyncPairRunId;
            dest.LastSyncPairResultId = src.LastSyncPairResultId;
            dest.Name = src.Name;
            dest.LocalFolderPath = src.LocalFolderPath;
            dest.Mode = src.Mode;
            dest.CompareType = src.CompareType;
            dest.ConflictHandlingType = src.ConflictHandlingType;

            if (!dest.ServerPath.BothNullOrSequenceEqual(src.ServerPath)) dest.ServerPath = src.ServerPath;
            if (!dest.AllowList.BothNullOrSequenceEqual(src.AllowList)) dest.AllowList = src.AllowList;
            if (!dest.DenyList.BothNullOrSequenceEqual(src.DenyList)) dest.AllowList = src.DenyList;

            return dest;
        }

        public static SyncPairRun Update(this SyncPairRun dest, SyncPairRun src)
        {
            if (dest == null || src == null) return src;

            dest.Id = src.Id;
            dest.State = src.State;
            dest.CurrentCount = src.CurrentCount;
            dest.AllFilesCount = src.AllFilesCount;
            dest.ComparedFilesCount = src.ComparedFilesCount;
            dest.EqualFilesCount = src.EqualFilesCount;
            dest.ConflictFilesCount = src.ConflictFilesCount;
            dest.CopiedLocalFilesCount = src.CopiedLocalFilesCount;
            dest.CopiedServerFilesCount = src.CopiedServerFilesCount;
            dest.DeletedLocalFilesCount = src.DeletedLocalFilesCount;
            dest.DeletedServerFilesCount = src.DeletedServerFilesCount;
            dest.ErrorFilesCount = src.ErrorFilesCount;
            dest.IgnoreFilesCount = src.IgnoreFilesCount;
            dest.LocalFolderPath = src.LocalFolderPath;
            dest.CurrentQueryFolderRelPath = src.CurrentQueryFolderRelPath;
            dest.CurrentCopyToLocalRelPath = src.CurrentCopyToLocalRelPath;
            dest.CurrentCopyToServerRelPath = src.CurrentCopyToServerRelPath;
            dest.DeletedServerFilesCount = src.DeletedServerFilesCount;
            dest.DeletedLocalFilesCount = src.DeletedLocalFilesCount;

            return dest;
        }
    }
}

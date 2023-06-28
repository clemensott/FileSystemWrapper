using System.Linq;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct SyncPairResponseInfo
    {
        public string RunToken { get; set; }

        public SyncPairHandlerState State { get; set; }

        public int CurrentCount { get; set; }

        public int TotalCount { get; set; }

        public FilePairInfo[] ComparedFiles { get; set; }

        public FilePairInfo[] EqualFiles { get; set; }

        public FilePairInfo[] ConflictFiles { get; set; }

        public FilePairInfo[] CopiedLocalFiles { get; set; }

        public FilePairInfo[] CopiedServerFiles { get; set; }

        public FilePairInfo[] DeletedLocalFiles { get; set; }

        public FilePairInfo[] DeletedServerFiles { get; set; }

        public ErrorFilePairInfo[] ErrorFiles { get; set; }

        public string CurrentQueryFolderRelPath { get; set; }

        public FilePairInfo? CurrentCopyToLocalFile { get; set; }

        public FilePairInfo? CurrentCopyToServerFile { get; set; }

        public FilePairInfo? CurrentDeleteFromServerFile { get; set; }

        public FilePairInfo? CurrentDeleteFromLocalFile { get; set; }

        internal static SyncPairResponseInfo FromHandler(SyncPairHandler handler)
        {
            return new SyncPairResponseInfo()
            {
                State = handler.State,
                CurrentCount = handler.CurrentCount,
                TotalCount = handler.TotalCount,
                ComparedFiles = handler.ComparedFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                EqualFiles = handler.EqualFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                ConflictFiles = handler.ConflictFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                CopiedLocalFiles = handler.CopiedLocalFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                CopiedServerFiles = handler.CopiedServerFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                DeletedLocalFiles = handler.DeletedLocalFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                DeletedServerFiles = handler.DeletedServerFiles.Select(FilePairInfo.FromFilePair).ToArray(),
                ErrorFiles = handler.ErrorFiles.Select(ErrorFilePairInfo.FromFilePair).ToArray(),
                CurrentQueryFolderRelPath = handler.CurrentQueryFolderRelPath,
                CurrentCopyToLocalFile = FilePairInfo.FromFilePair(handler.CurrentCopyToLocalFile),
                CurrentCopyToServerFile = FilePairInfo.FromFilePair(handler.CurrentCopyToServerFile),
                CurrentDeleteFromServerFile = FilePairInfo.FromFilePair(handler.CurrentDeleteFromServerFile),
                CurrentDeleteFromLocalFile = FilePairInfo.FromFilePair(handler.CurrentDeleteFromLocalFile),
            };
        }
    }
}

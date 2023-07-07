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

        public FilePairInfo[] IgnoreFiles { get; set; }

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

        public static SyncPairResponseInfo FromHandler(SyncPairHandler handler)
        {
            return new SyncPairResponseInfo()
            {
                RunToken = handler.RunToken,
                State = handler.State,
                CurrentCount = handler.CurrentCount,
                TotalCount = handler.TotalCount,
                ComparedFiles = handler.ComparedFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                EqualFiles = handler.EqualFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                IgnoreFiles = handler.IgnoreFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                ConflictFiles = handler.ConflictFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                CopiedLocalFiles = handler.CopiedLocalFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                CopiedServerFiles = handler.CopiedServerFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                DeletedLocalFiles = handler.DeletedLocalFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
                DeletedServerFiles = handler.DeletedServerFiles.Select(FilePairInfo.FromFilePair).OfType<FilePairInfo>().ToArray(),
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

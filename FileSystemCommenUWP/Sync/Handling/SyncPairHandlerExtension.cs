using FileSystemCommonUWP.Sync.Handling.Communication;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileSystemCommonUWP.Sync.Handling
{
    public static class SyncPairHandlerExtension
    {
        public static SyncPairResponseInfo ToResponse(this SyncPairHandler handler)
        {
            FilePairInfo?[] comparedFiles, equalFiles, ignoreFiles, conflictFiles, copiedLocalFiles, copiedServerFiles, deletedLocalFiles, deletedServerFiles;
            ErrorFilePairInfo[] errorFiles;

            //lock (handler)
            //{
            //    comparedFiles = handler.ComparedFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    equalFiles = handler.EqualFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    ignoreFiles = handler.IgnoreFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    conflictFiles = handler.ConflictFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    copiedLocalFiles = handler.CopiedLocalFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    copiedServerFiles = handler.CopiedServerFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    deletedLocalFiles = handler.DeletedLocalFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    deletedServerFiles = handler.DeletedServerFiles.Select(FilePairInfo.FromFilePair).ToArray();
            //    errorFiles = handler.ErrorFiles.Select(ErrorFilePairInfo.FromFilePair).ToArray();
            //}

            return new SyncPairResponseInfo()
            {
                //RunToken = handler.RunToken,
                //State = handler.State,
                //CurrentCount = handler.CurrentCount,
                //TotalCount = handler.TotalCount,
                //ComparedFiles = new ObservableCollection<FilePairInfo>(comparedFiles.Cast<FilePairInfo>()),
                //EqualFiles = new ObservableCollection<FilePairInfo>(equalFiles.Cast<FilePairInfo>()),
                //IgnoreFiles = new ObservableCollection<FilePairInfo>(ignoreFiles.Cast<FilePairInfo>()),
                //ConflictFiles = new ObservableCollection<FilePairInfo>(conflictFiles.Cast<FilePairInfo>()),
                //CopiedLocalFiles = new ObservableCollection<FilePairInfo>(copiedLocalFiles.Cast<FilePairInfo>()),
                //CopiedServerFiles = new ObservableCollection<FilePairInfo>(copiedServerFiles.Cast<FilePairInfo>()),
                //DeletedLocalFiles = new ObservableCollection<FilePairInfo>(deletedLocalFiles.Cast<FilePairInfo>()),
                //DeletedServerFiles = new ObservableCollection<FilePairInfo>(deletedServerFiles.Cast<FilePairInfo>()),
                //ErrorFiles = new ObservableCollection<ErrorFilePairInfo>(errorFiles),
                //CurrentQueryFolderRelPath = handler.CurrentQueryFolderRelPath,
                //CurrentCopyToLocalFile = FilePairInfo.FromFilePair(handler.CurrentCopyToLocalFile),
                //CurrentCopyToServerFile = FilePairInfo.FromFilePair(handler.CurrentCopyToServerFile),
                //CurrentDeleteFromServerFile = FilePairInfo.FromFilePair(handler.CurrentDeleteFromServerFile),
                //CurrentDeleteFromLocalFile = FilePairInfo.FromFilePair(handler.CurrentDeleteFromLocalFile),
            };
        }
    }
}

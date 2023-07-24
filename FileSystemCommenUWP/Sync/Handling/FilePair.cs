using FileSystemCommon;
using System.IO;
using Windows.Storage;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class FilePair
    {
        public string Name { get; }

        public string RelativePath { get; }

        public string ServerFullPath { get; }

        public bool ServerFileExists { get; }

        public StorageFile LocalFile { get; }

        public object ServerCompareValue { get; set; }

        public object LocalCompareValue { get; set; }

        public FilePair(string name, string relativePath, string serverFullPath, bool serverFileExists, StorageFile localFile)
        {
            Name = name;
            RelativePath = relativePath;
            ServerFullPath = serverFullPath;
            ServerFileExists = serverFileExists;
            LocalFile = localFile;
        }
    }
}

using System.IO;
using Windows.Storage;

namespace FileSystemUWP.Sync.Handling
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

        public FilePair(string serverBasePath, string relPath, StorageFile localFile, bool serverFileExists)
        {
            RelativePath = relPath.Trim('\\');
            ServerFullPath = Path.Combine(serverBasePath, relPath);
            Name = Path.GetFileName(ServerFullPath);
            LocalFile = localFile;
            ServerFileExists = serverFileExists;
        }
    }
}

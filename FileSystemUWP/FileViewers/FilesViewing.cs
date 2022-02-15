using FileSystemUWP.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemUWP.FileViewers
{
    class FilesViewing
    {
        public FileSystemItem CurrentFile { get; set; }

        public IEnumerable<FileSystemItem> Files { get; }

        public Api Api { get; }

        public FilesViewing(FileSystemItem currentFile, IEnumerable<FileSystemItem> files, Api api)
        {
            CurrentFile = currentFile;
            Files = files;
            Api = api;
        }
    }
}

using FileSystemCommonUWP.API;
using FileSystemUWP.Models;
using System.Collections.Generic;

namespace FileSystemUWP.FileViewers
{
    class FilesViewing
    {
        public bool ResumePlayback { get; }

        public FileSystemItem CurrentFile { get; set; }

        public IEnumerable<FileSystemItem> Files { get; }

        public Api Api { get; }

        public FilesViewing(bool resumePlayback, FileSystemItem currentFile, IEnumerable<FileSystemItem> files, Api api)
        {
            ResumePlayback = resumePlayback;
            CurrentFile = currentFile;
            Files = files;
            Api = api;
        }
    }
}

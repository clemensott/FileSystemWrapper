﻿namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct FilePairInfo
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public string ServerFullPath { get; set; }

        public bool ServerFileExists { get; set; }

        public string LocalFilePath { get; set; }

        internal static FilePairInfo FromFilePair(FilePair pair)
        {
            return new FilePairInfo()
            {
                Name = pair.Name,
                RelativePath = pair.RelativePath,
                ServerFullPath = pair.ServerFullPath,
                ServerFileExists = pair.ServerFileExists,
                LocalFilePath = pair.LocalFile.Path,
            };
        }
    }
}

﻿namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct ErrorFilePairInfo
    {
        public FilePairInfo File { get; set; }

        public string Message { get; set; }

        public string Stacktrace { get; set; }

        public string Exception { get; set; }

        public static ErrorFilePairInfo FromFilePair(ErrorFilePair pair)
        {
            return new ErrorFilePairInfo()
            {
                File = FilePairInfo.FromFilePair(pair.Pair).Value,
                Message = pair.Exception.Message,
                Stacktrace = pair.Exception.StackTrace,
                Exception = pair.Exception.ToString(),
            };
        }
    }
}

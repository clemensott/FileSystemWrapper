using System;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class ErrorFilePair
    {
        public FilePair Pair { get; }

        public Exception Exception { get; }

        public ErrorFilePair(FilePair pair, Exception exception)
        {
            Pair = pair;
            Exception = exception;
        }
    }
}

using System;

namespace FileSystemCommonUWP.Sync.Handling.Progress
{
    public struct SyncPairProgressErrorFileUpdate
    {
        public string RelativePath { get; }

        public string ErrorMessage { get; }

        public string ErrorStacktrace { get; }

        public string ErrorException { get; }

        public SyncPairProgressErrorFileUpdate(string relativePath, string message, Exception exception) : this()
        {
            RelativePath = relativePath;
            ErrorMessage = message;
            ErrorStacktrace = exception?.StackTrace;
            ErrorException = exception?.ToString();
        }
    }
}

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct ErrorFilePairInfo
    {
        public FilePairInfo Pair { get; set; }

        public string Message { get; set; }

        public string Stacktrace { get; set; }

        public string Exception { get; set; }

        internal static ErrorFilePairInfo FromFilePair(ErrorFilePair pair)
        {
            return new ErrorFilePairInfo()
            {
                Pair = FilePairInfo.FromFilePair(pair.Pair),
                Message = pair.Exception.Message,
                Stacktrace = pair.Exception.StackTrace,
                Exception = pair.Exception.ToString(),
            };
        }
    }
}

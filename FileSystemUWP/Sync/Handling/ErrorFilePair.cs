using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Handling
{
    class ErrorFilePair
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

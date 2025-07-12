using System.Net;
using System;

namespace FileSystemWeb.Models.Exceptions
{
    public class NotFoundException : HttpException
    {
        public NotFoundException(string message, int code) : this(message, code, null)
        {
        }

        public NotFoundException(string message, int code, Exception innerException)
            : base(HttpStatusCode.NotFound, message, code, innerException)
        {
        }
    }
}

﻿using System.Net;
using System;

namespace FileSystemWeb.Models.Exceptions
{
    public class UnauthorizedException : HttpException
    {
        public UnauthorizedException(string message, int code) : this(message, code, null)
        {
        }

        public UnauthorizedException(string message, int code, Exception innerException)
            : base(HttpStatusCode.Unauthorized, message, code, innerException)
        {
        }
    }
}

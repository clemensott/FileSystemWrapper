﻿using System.Net;
using System;

namespace FileSystemWeb.Models.Exceptions
{
    public class BadRequestException : HttpException
    {
        public BadRequestException(string message, int code)
            : this(message, code, null)
        {
        }

        public BadRequestException(string message, int code, Exception innerException)
            : base(HttpStatusCode.BadRequest, message, code, innerException)
        {
        }
    }
}

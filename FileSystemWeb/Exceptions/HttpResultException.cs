using System;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Exceptions
{
    public class HttpResultException : Exception
    {
        public ActionResult Result { get; }

        public HttpResultException(ActionResult result)
        {
            Result = result;
        }

        public static explicit operator HttpResultException(ActionResult result)
        {
            return new HttpResultException(result);
        }

        public static explicit operator ActionResult(HttpResultException exception)
        {
            return exception.Result;
        }
    }
}

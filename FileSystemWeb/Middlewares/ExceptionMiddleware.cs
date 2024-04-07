using Microsoft.AspNetCore.Http;
using StdOttStandard.Models.HttpExceptions;
using System.Threading.Tasks;
using System;

namespace FileSystemWeb.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                if (e is not HttpException httpException)
                {
                    httpException = new InternalServerErrorHttpException(1000);
                }

                context.Response.StatusCode = (int)httpException.Status;
                await context.Response.WriteAsJsonAsync(new
                {
                    status = (int)httpException.Status,
                    message = httpException.Message,
                    code = httpException.Code,
                });
            }
        }
    }
}

using FileSystemWeb.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FileSystemWeb.Extensions.Middlewares
{
    static class ErrorHandlingMiddlewareExtensions
    {
        public static void UseGlobalErrorHandler(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            else app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

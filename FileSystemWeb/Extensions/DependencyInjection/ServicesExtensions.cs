using FileSystemWeb.Services.File;
using FileSystemWeb.Services.Folder;
using FileSystemWeb.Services.Share;
using Microsoft.Extensions.DependencyInjection;

namespace FileSystemWeb.Extensions.DependencyInjection
{
    static class ServicesExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<BigFileService>();
            services.AddScoped<FileService>();
            services.AddScoped<FolderContentService>();
            services.AddScoped<FolderService>();
            services.AddScoped<ShareFileService>();
            services.AddScoped<ShareFolderService>();
            services.AddScoped<ShareService>();
        }
    }
}

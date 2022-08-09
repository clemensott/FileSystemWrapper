using System.IO;
using System.Security.Claims;
using FileSystemWeb.Constants;
using FileSystemWeb.Data;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using static FileSystemWeb.Constants.Permissions;

namespace FileSystemWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(config => { config.UseSqlite(@"Data Source=.\auth.db;"); });

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                string[] permissions = new string[]
                {
                    Share.GetShareFiles,
                    Share.PostShareFile,
                    Share.GetShareFile,
                    Share.PutShareFile,
                    Share.DeleteShareFile,
                    Share.GetShareFolders,
                    Share.PostShareFolder,
                    Share.GetShareFolder,
                    Share.PutShareFolder,
                    Share.DeleteShareFolder,
                    Users.GetAllUsers,
                    Users.PostUser,
                };
                foreach (string permission in permissions)
                {
                    options.AddPolicy(permission, policy => policy.RequireClaim(CustomClaimTypes.Permission, permissions));
                }
            });

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "fs_login";
                config.Cookie.HttpOnly = true;
            });

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (!env.IsDevelopment())
            {
                string sslCertificateDirPath = Path.Combine(Directory.GetCurrentDirectory(), @".well-known");
                if (!Directory.Exists(sslCertificateDirPath)) Directory.CreateDirectory(sslCertificateDirPath);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(sslCertificateDirPath),
                    RequestPath = new PathString("/.well-known"),
                    ServeUnknownFileTypes = true // serve extensionless files
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
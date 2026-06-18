using Dotnet.Models;
using Dotnet.Security;
using Microsoft.EntityFrameworkCore;

namespace Dotnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add DbContext with  connection string
            builder.Services.AddDbContext<CrudContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("dbConn")).EnableSensitiveDataLogging());

            // Register DataSecurity provider
            builder.Services.AddSingleton<DataSecurityProvider>();

            // Add authentication and session services 
         //   builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=static}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

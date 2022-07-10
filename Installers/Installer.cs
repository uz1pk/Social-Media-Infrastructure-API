using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TweetAPI.Data;
using Microsoft.OpenApi.Models;
using TweetAPI.Services;

namespace TweetAPI.Installers
{
    public static class Installer
    {
        public static void ServicesInit(this IServiceCollection services, IConfiguration config)
        {
            //Add services to the container.
            string? connectionString = config.GetConnectionString("DefaultConnection");


            services.AddDbContext<DBContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();


            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DBContext>();

            services.AddScoped<IPostService, PostService>();

            services.AddSwaggerGen(temp =>
            {
                temp.SwaggerDoc("v1", new OpenApiInfo { Title = "Tweet REST API", Version = "v1" });
            });
        }
    }
}

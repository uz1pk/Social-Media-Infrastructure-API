using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TweetAPI.Data;
using Microsoft.OpenApi.Models;
using TweetAPI.Services;
using TweetAPI.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            var jwtSettings = new JwtSettings();
            config.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            services.AddScoped<IPostService, PostService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = false,
                        ValidateLifetime = true,
                    };
                });

            services.AddSwaggerGen(temp =>
            {
                temp.SwaggerDoc("v1", new OpenApiInfo { Title = "Tweet REST API", Version = "v1" });

                //VERIFY SCHEME, CHECK THAT IT WORKS
                temp.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Auth using the bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // MAKE SURE TO VERIFY THIS SECURITY REQ
                temp.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    { 
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearAuth" 
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, 
                        new List<string>()
                    }
                });

            });

        }
    }
}

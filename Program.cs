using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TweetAPI.Data;
using TweetAPI.Config;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DBContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(temp =>
{
    temp.SwaggerDoc("v1", new OpenApiInfo { Title = "Tweet REST API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}

var swagConfig = new SwaggerOptions();
builder.Configuration.GetSection(nameof(SwaggerOptions)).Bind(swagConfig);

app.UseSwaggerUI(config =>
{
    config.SwaggerEndpoint(swagConfig.UIEndpoint, swagConfig.Description);
});
app.UseSwagger(config =>
{
    config.RouteTemplate = swagConfig.JsonRoute;
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

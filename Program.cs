using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TweetAPI.Config;
using TweetAPI.Data;
using TweetAPI.Installers;
using TweetAPI.Options;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.ServicesInit(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigrationsEndPoint();

//if (app.Environment.IsDevelopment())
//{
//    app.UseMigrationsEndPoint();
//}
//else
//{
//    //app.UseHsts();
//}
    

var swagConfig = new SwaggerOptions();
builder.Configuration.GetSection(nameof(SwaggerOptions)).Bind(swagConfig);

app.UseSwagger(config =>
{
    config.RouteTemplate = swagConfig.JsonRoute;
});
app.UseSwaggerUI(config =>
{
    config.SwaggerEndpoint(swagConfig.UIEndpoint, swagConfig.Description);
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
}

app.Run();

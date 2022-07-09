using TweetAPI.Config;
using TweetAPI.Installers;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.ServicesInit(builder.Configuration);

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

SwaggerOptions swagConfig = new SwaggerOptions();
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

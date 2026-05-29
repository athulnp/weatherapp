using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using WeatherApp.IService;
using WeatherApp.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  
builder.Services.AddControllersWithViews();

// Register DI Services  
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();


// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddHttpClient("weatherClient", client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org/");
}).ConfigureHttpMessageHandlerBuilder(builder =>
{
    // Disable SSL certificate validation for development
    if (!builder.Name.Contains("production"))
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        builder.PrimaryHandler = handler;
    }
});

// Add Auth0 authentication only if configuration is present
var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0ClientId = builder.Configuration["Auth0:ClientId"];

if (!string.IsNullOrEmpty(auth0Domain) && !string.IsNullOrEmpty(auth0ClientId))
{
    builder.Services.AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = auth0Domain;
        options.ClientId = auth0ClientId;
        options.CallbackPath = new PathString("/callback");
    });

    // Configure cookie settings separately  
    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/access-denied";
    });
}

builder.Services.AddMemoryCache();



var app = builder.Build();

// Configure the HTTP request pipeline.  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHttpsRedirection();
    // 
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

// Use authentication and authorization only if Auth0 is configured
if (!string.IsNullOrEmpty(auth0Domain) && !string.IsNullOrEmpty(auth0ClientId))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

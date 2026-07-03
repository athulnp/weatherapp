using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WeatherApp.Data;
using WeatherApp.IService;
using WeatherApp.Service;
using System.IO.Compression;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure supported cultures
// Both the region-specific cultures (used by the .resx files) and the short
// neutral codes that appear in the URL prefix (/hi, /ta, /ml) are supported so
// the RouteDataRequestCultureProvider can resolve either form.
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("en-US"),
    new CultureInfo("hi"),
    new CultureInfo("hi-IN"),
    new CultureInfo("ta"),
    new CultureInfo("ta-IN"),
    new CultureInfo("ml"),
    new CultureInfo("ml-IN")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear();
    // 1. URL path prefix (/hi, /ta, /ml) -> the {culture} route value. This is the
    //    authoritative signal so each language has its own crawlable URL.
    options.RequestCultureProviders.Add(new RouteDataRequestCultureProvider { RouteDataStringKey = "culture", UIRouteDataStringKey = "culture" });
    // 2. Fallbacks for non-prefixed requests.
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
});

// Add Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=weatherapp.db"));

// Register DI Services  
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<CitySearchService>();
builder.Services.AddHttpClient<CountryCitiesService>();

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/css",
        "application/javascript",
        "image/svg+xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});


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

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    ArticleSeeder.SeedArticles(context);
}

// Configure the HTTP request pipeline.  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHttpsRedirection();
    // 
    app.UseHsts();
}

// Security & SEO-friendly response headers
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(self), camera=(), microphone=()";
    await next();
});

// Add Response Compression
app.UseResponseCompression();

// Add Static Files with Caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 1 year
        var headers = ctx.Context.Response.GetTypedHeaders();
        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(365)
        };
    }
});

app.UseRouting();

// Map Health Check endpoint
app.MapHealthChecks("/health");

// Use Request Localization
app.UseRequestLocalization();

// Use authentication and authorization only if Auth0 is configured
if (!string.IsNullOrEmpty(auth0Domain) && !string.IsNullOrEmpty(auth0ClientId))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers(); // Enable attribute routing for CitiesController

// Language-prefixed routes (/hi, /ta, /ml). The {culture} segment is constrained
// to the supported non-English codes so it never swallows normal paths, and it
// feeds RouteDataRequestCultureProvider so each language is a distinct URL.
app.MapControllerRoute(
    name: "localized",
    pattern: "{culture:regex(^(hi|ta|ml)$)}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.VM;
using BiblioMit.Pwa;
using BiblioMit.Services;
using BiblioMit.Services.Hubs;
using BiblioMit.Services.Interfaces;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;

string os = Environment.OSVersion.Platform.ToString();
bool plankton = args.Any(a => a == "plankton");
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    config.AddJsonFile($"appsettings.{os}.json", optional: true, reloadOnChange: true));

builder.WebHost
    .UseKestrel(c =>
    {
        c.AddServerHeader = false;
        c.Limits.MaxConcurrentConnections = 200;
        c.Limits.MaxConcurrentUpgradedConnections = 200;
    });

//builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
//if(plankton) os = "Remote";
string connectionString = builder.Configuration.GetConnectionString($"{os}Connection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString
    , o =>
    {
        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        o.CommandTimeout(10_000);
        o.EnableRetryOnFailure();
    }
    ));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>()
    ;

//builder.Services.AddAuthentication(
//    CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    //This setting is in csproj
    //options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    // Formatting numbers, dates, etc.
    options.SupportedCultures = Statics.SupportedCultures;
    // UI strings that we have localized.
    options.SupportedUICultures = Statics.SupportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

builder.Services.AddResponseCaching();

builder.Services.AddControllersWithViews(
//    options => {
// this requires all GETS to have antiforgery token
//    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
// this would only allow authenticated users by default
//    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
//}
    )
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    ;

// this enables compatibility with different browsers
//builder.Services.ConfigureNonBreakingSameSiteCookies();

builder.Services.AddHsts(
    options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.ExcludedHosts.Add("bibliomit.cl");
    options.ExcludedHosts.Add("www.bibliomit.cl");
}
);

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

//this is for sendgrid
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

//Adds claims to authorization policies
builder.Services.AddAuthorization(options =>
{
    UserClaims.Banners.Enum2ListNames().ForEach(item =>
        options.AddPolicy(item, policy => policy.RequireClaim(item, item))
    );
});

builder.Services.Configure<FlowSettings>(o =>
{
    bool dev = !args.Any(a => a == "flow");
    string flowEnv = dev ? "Sandbox" : "Production";
    string preffix = dev ? "sandbox" : "www";
    o.ApiKey = builder.Configuration[$"Flow:{flowEnv}:ApiKey"];
    o.SecretKey = builder.Configuration[$"Flow:{flowEnv}:SecretKey"];
    o.Currency = "CLP";
    o.EndPoint = new Uri($"https://{preffix}.flow.cl/api");
});
builder.Services.AddScoped<IFlow, FlowService>();

builder.Services.AddSignalR(options =>
    options.EnableDetailedErrors = true);

Libman.LoadJson();
CSPTag.Start();

builder.Services.AddProgressiveWebApp(new PwaOptions
{
    RegisterServiceWorker = true,
    RoutesToPreCache = "/Home/Index"
});

bool seed = args.Any(a => a == "seed");
if (seed)
{
    builder.Services.AddHostedService<SeedBackground>();
    builder.Services.AddScoped<ISeed, SeedService>();
    builder.Services.AddScoped<IUpdateJsons, UpdateJsons>();
}
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IUrlHelper>(x =>
{
    ActionContext? actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
    if (actionContext == null) throw new NullReferenceException(nameof(actionContext));
    IUrlHelperFactory factory = x.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext);
});

builder.Services.AddScoped<IPuppet, PuppetService>();
builder.Services.AddSingleton(new PlanktonArguments { Run = plankton });
builder.Services.AddHostedService<PlanktonBackground>();
builder.Services.AddScoped<IPlanktonService, PlanktonService>();

builder.Services.AddScoped<INodeService, NodeService>();
builder.Services.AddScoped<IBannerService, BannerService>();
//??
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IImport, ImportService>();
builder.Services.AddScoped<ITableToExcel, TableToExcelService>();

builder.Services.AddAntiforgery(options =>
{
    options.FormFieldName = "__RequestVerificationToken";
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddScoped<IForum, ForumService>();
builder.Services.AddScoped<IEntryHub, EntryHub>();
builder.Services.AddScoped<IPost, PostService>();

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//    //options.Cookie.Name = "BiblioMit";
//    options.Cookie.HttpOnly = true;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//    options.LoginPath = "/Identity/Account/Login";
//    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
//    options.SlidingExpiration = true;
//});

//Host
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
}

//app.UseCookiePolicy(
//    new CookiePolicyOptions
//{
//    Secure = CookieSecurePolicy.Always,
//    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
//    MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None
//}
//    );

app.UseRedirectValidation(opts =>
{
    opts.AllowSameHostRedirectsToHttps();
    opts.AllowedDestinations("https://sandbox.flow.cl/app/web/pay.php");
});

app.UseReferrerPolicy(opts => opts.NoReferrer());

FileExtensionContentTypeProvider provider = new();
provider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseHttpsRedirection();

app.UseCors();
//this is unnecessary it serves index.html and default.html
//app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        const int durationInSecond = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSecond;
    }
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRouting();

app.UseResponseCaching();

IOptions<RequestLocalizationOptions>? localOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (localOptions != null) app.UseRequestLocalization(localOptions.Value);

app.UseXContentTypeOptions();
app.UseXfo(xfo => xfo.Deny());
app.UseXXssProtection(options => options.EnabledWithBlockMode());

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToController("Index", "Home");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
app.MapControllerRoute(
    name: "culture-route",
    pattern: "{culture=es}/{controller=Home}/{action=Index}/{id?}").RequireAuthorization();

app.MapHub<EntryHub>("/entryHub").RequireAuthorization();
app.MapRazorPages();

//app.MapGet("/", () => DateTime.Now.Millisecond);

app.Run();

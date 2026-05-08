using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;
using NipponQuest.Services;
using Quartz;
using Microsoft.AspNetCore.HttpOverrides; // Added for Forwarded Headers

var builder = WebApplication.CreateBuilder(args);

// --- 0. FILE UPLOAD, FORM & COLLECTION LIMITS ---
const long MaxFileSize = 262144000;
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = MaxFileSize;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxFileSize;
    options.ValueCountLimit = 30000;
    options.ValueLengthLimit = int.MaxValue;
});

// --- 1. SQLITE INITIALIZATION ---
SQLitePCL.Batteries_V2.Init();

// --- 2. DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- 3. IDENTITY SETUP ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// --- GOOGLE AUTHENTICATION & PROXY/TUNNEL FIXES ---

// 1. Configure Forwarded Headers for Dev Tunnels
// This tells the app to trust the HTTPS protocol reported by the tunnel
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Tunnels change the host/proto dynamically, so we clear these to allow any proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2. Cookie Policy for SameSite (Fixes Correlation Failed)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

// 3. External Cookie Configuration for cross-site redirects
builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = googleClientId;
            googleOptions.ClientSecret = googleClientSecret;
            // Ensures the correlation cookie is marked as secure
            googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        });
}

// --- 4. CUSTOM SERVICE REGISTRATIONS ---
builder.Services.AddScoped<GithubService>();

builder.Services.AddControllersWithViews(options =>
{
    options.MaxModelBindingCollectionSize = 10000;
});

// --- 5. QUARTZ BACKGROUND JOBS ---
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("WeeklyLeagueResetJob");
    q.AddJob<NipponQuest.Jobs.WeeklyLeagueResetJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("WeeklyLeagueResetJob-trigger")
        .WithCronSchedule("0 0 0 ? * MON"));

    var streakJobKey = new JobKey("StreakDecayJob");
    q.AddJob<NipponQuest.Jobs.StreakDecayJob>(opts => opts.WithIdentity(streakJobKey));
    q.AddTrigger(opts => opts
        .ForJob(streakJobKey)
        .WithIdentity("StreakDecayJob-trigger")
        .WithCronSchedule("0 5 0 * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddHostedService<AIKanaGeneratorService>();

// --- 6. BUILD APP ---
var app = builder.Build();

// --- 7. SEED DATA AND MIGRATIONS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        SeedData.Initialize(services);
        DbInitializer.SeedKanaBlitzData(context);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database seeding error.");
        throw;
    }
}

// --- 8. MIDDLEWARE PIPELINE ---

// CRITICAL: Forwarded Headers MUST be at the very top of the pipeline
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CookiePolicy must be before Authentication
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<NipponQuest.Middleware.LoginStreakMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// Helper method for SameSite Browser Compatibility
void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        // Handle browsers that do not support SameSite=None
        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
        {
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}
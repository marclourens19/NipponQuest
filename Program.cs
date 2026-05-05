using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;
using NipponQuest.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// --- 0. FILE UPLOAD, FORM & COLLECTION LIMITS ---
// Configured to handle 250MB files and up to 30,000 form fields.
const long MaxFileSize = 262144000;
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = MaxFileSize;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxFileSize;
    options.ValueCountLimit = 30000; // Handles the ~14,000 fields in the Core 2000 deck
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

// >>> ADDED GOOGLE AUTHENTICATION HERE <<<
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // This will pull the keys from the JSON you provided in appsettings.json
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// --- 4. CUSTOM SERVICE REGISTRATIONS ---
builder.Services.AddScoped<GithubService>();

// UPDATE: Increased MaxModelBindingCollectionSize to allow 2000+ cards in the list
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
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// --- 6. BUILD APP ---
var app = builder.Build();

// --- 7. SEED DATA AND MIGRATIONS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// --- 8. MIDDLEWARE PIPELINE ---
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
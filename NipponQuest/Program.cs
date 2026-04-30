using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Jobs;
using NipponQuest.Models;
using NipponQuest.Services;
using Quartz;

// PHASE 1 FIX: Initialize SQLite native drivers immediately on app startup
// to prevent "Process Exited with Code -1" during later imports.
SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

// Database & Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Google Authentication
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.AddControllersWithViews();

// >>> UPLOAD LIMITS (500 MB) <<<
// Essential for large Anki packages (.apkg)
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 524288000);
builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 524288000);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000;
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = 2097152;
});

// Quartz.NET Reset Job
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("WeeklyLeagueResetJob");
    q.AddJob<WeeklyLeagueResetJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts.ForJob(jobKey).WithIdentity("WeeklyResetTrigger").WithCronSchedule("0 0 0 ? * SUN"));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddScoped<GithubService>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); }
else { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider);
}

app.Run();
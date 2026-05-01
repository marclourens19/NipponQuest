using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;
using NipponQuest.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// --- 1. MOVE INITIALIZATION HERE ---
// Initialize SQLite support after the builder is created but before services are added.
SQLitePCL.Batteries.Init();

// 2. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 3. Identity Setup
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 4. Custom Service Registrations
builder.Services.AddScoped<GithubService>();

builder.Services.AddControllersWithViews();

// 5. Quartz Background Jobs
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

var app = builder.Build();

// 6. Seed Data and Migrations
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

// 7. Middleware Pipeline
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
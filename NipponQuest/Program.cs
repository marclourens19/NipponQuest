using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Jobs;
using NipponQuest.Models;
using NipponQuest.Services;
using Quartz;

namespace NipponQuest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configuring Identity services with our custom ApplicationUser class
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // --- GOOGLE AUTHENTICATION CONFIGURATION ---
            // This pulls the ClientId and Secret from your User Secrets
            builder.Services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                });

            builder.Services.AddControllersWithViews();

            // --- QUARTZ.NET WEEKLY RESET CONFIGURATION ---
            builder.Services.AddQuartz(q =>
            {
                var jobKey = new JobKey("WeeklyLeagueResetJob");
                q.AddJob<WeeklyLeagueResetJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("WeeklyLeagueResetTrigger")
                    .WithCronSchedule("0 0 0 ? * SUN"));
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Add this line with the other service registrations
            builder.Services.AddScoped<GithubService>();

            // ---------------------------------------------

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
            app.UseRouting();

            app.UseAuthentication(); // Ensure Authentication is called before Authorization
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            // Seed the database with initial data.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedData.Initialize(services);
            }

            app.Run();
        }
    }
}
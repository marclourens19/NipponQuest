using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Models;
using NipponQuest.Data; // Ensure this matches your Data folder namespace

namespace NipponQuest.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Added field for DB access

        // Inject both UserManager and ApplicationDbContext
        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Single merged Index method to handle DB stats
        public async Task<IActionResult> Index()
        {
            // Sum total XP from all users in the database
            // Note: Use 'TotalEXP' or 'ExperiencePoints' depending on your actual ApplicationUser property name
            ViewBag.TotalXP = await _context.Users.SumAsync(u => u.TotalEXP);

            // Count users who have an active login streak
            // Note: Use 'LoginStreak' to match your ApplicationUser property
            ViewBag.ActiveStreaks = await _context.Users.CountAsync(u => u.LoginStreak > 0);

            return View();
        }

        // --- DEV TOOL ENGINE: GOD MODE COMMANDS ---

        [HttpPost]
        public async Task<IActionResult> DevUpdateXP(int amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentXP += amount;
                user.TotalEXP += amount;
                user.WeeklyXP += amount;

                while (user.CurrentXP >= user.RequiredXP)
                {
                    user.CurrentXP -= user.RequiredXP;
                    user.Level++;
                }

                if (user.CurrentXP < 0) user.CurrentXP = 0;
                if (user.TotalEXP < 0) user.TotalEXP = 0;
                if (user.WeeklyXP < 0) user.WeeklyXP = 0;

                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DevUpdateGold(int amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.Gold += amount;
                if (user.Gold < 0) user.Gold = 0;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DevSetStreak(int streak)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.LoginStreak = streak;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DevSetLeague(LeagueRank rank)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentLeague = rank;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy() => View();

        public IActionResult Terms() => View();

        public IActionResult Cookies() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
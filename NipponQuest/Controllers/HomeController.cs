using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- DEV TOOL: ADD 25 XP ---
        public async Task<IActionResult> AddTestXP()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentXP += 25;
                user.TotalEXP += 25;
                user.WeeklyXP += 25;

                while (user.CurrentXP >= user.RequiredXP)
                {
                    user.CurrentXP -= user.RequiredXP;
                    user.Level++;
                }

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        // --- DEV TOOL: ADD 100 XP ---
        public async Task<IActionResult> Add100TestXP()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentXP += 100;
                user.TotalEXP += 100;
                user.WeeklyXP += 100;

                while (user.CurrentXP >= user.RequiredXP)
                {
                    user.CurrentXP -= user.RequiredXP;
                    user.Level++;
                }

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        // --- DEV TOOL: ADD GOLD ---
        public async Task<IActionResult> AddTestGold()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.Gold += 100; // Adds 100 gold per click
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        // --- DEV TOOL: SET STREAK ---
        [HttpPost]
        public async Task<IActionResult> SetTestStreak(int streak)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.LoginStreak = streak;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        // --- DEV TOOL: SET RANK ---
        [HttpPost]
        public async Task<IActionResult> SetTestLeague(LeagueRank rank)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentLeague = rank;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
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

        public IActionResult Index() => View();

        // --- DEV TOOL ENGINE: GOD MODE COMMANDS ---

        [HttpPost]
        public async Task<IActionResult> DevUpdateXP(int amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.GamerTag == "Yugenclad")
            {
                // Apply change to all XP fields
                user.CurrentXP += amount;
                user.TotalEXP += amount;
                user.WeeklyXP += amount;

                // Handle Leveling Up (Loop in case of huge XP gain)
                while (user.CurrentXP >= user.RequiredXP)
                {
                    user.CurrentXP -= user.RequiredXP;
                    user.Level++;
                }

                // Safety: Prevent negative values if removing too much
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
                if (user.Gold < 0) user.Gold = 0; // Safety floor
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
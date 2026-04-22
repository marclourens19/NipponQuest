using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor injection to access the Database Users
        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- DEV TOOL ACTION ---
        public async Task<IActionResult> AddTestXP()
        {
            var user = await _userManager.GetUserAsync(User);

            // SECURITY: Only allows YOU to gain XP. 
            // Swap "Yugenclad" with whatever GamerTag you registered with.
            if (user != null && user.GamerTag == "Yugenclad")
            {
                user.CurrentXP += 25;
                user.TotalEXP += 25;

                // Level Up Logic: Checks if current XP exceeded the requirement
                while (user.CurrentXP >= user.RequiredXP)
                {
                    user.CurrentXP -= user.RequiredXP; // Carry over leftovers
                    user.Level++;
                }

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
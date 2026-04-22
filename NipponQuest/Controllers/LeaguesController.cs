using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using NipponQuest.Models;
using System.Threading.Tasks;

namespace NipponQuest.Controllers
{
    public class LeaguesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaguesController(UserManager<ApplicationUser> _userManager)
        {
            this._userManager = _userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            // In a real app, you'd calculate global rank here. 
            // For now, we pass the user model to the view.
            return View(user);
        }
    }
}
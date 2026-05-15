using Microsoft.AspNetCore.Mvc;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    // Routes to /KanaBattle/...
    [Route("[controller]/[action]")]
    public class KanaBattleController : Controller
    {
        // 1. Solo KanaBlitz view
        public IActionResult Index()
        {
            // Use explicit rooted path so ASP.NET does not search the wrong folder.
            return View("~/Views/KanaBattle/KanaBlitz_Index.cshtml");
        }

        // 2. Multiplayer setup/lobby
        public IActionResult MainView()
        {
            return View("~/Views/KanaBattle/KanaBlitz_View.cshtml");
        }

        // 3. Multiplayer arena
        public IActionResult Battle(string difficulty = "normal", string script = "hiragana")
        {
            var model = new KanaBattleModel
            {
                Difficulty = difficulty,
                Script = script
            };

            return View("~/Views/KanaBattle/KanaBattle_View.cshtml", model);
        }
    }
}

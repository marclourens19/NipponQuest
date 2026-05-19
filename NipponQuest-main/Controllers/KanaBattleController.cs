using Microsoft.AspNetCore.Mvc;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    // Routes to /KanaBattle/...
    [Route("[controller]/[action]")]
    public class KanaBattleController : Controller
    {
        // ── 1. Hub / Mode-Select ──
        // GET /KanaBattle/Index
        // Renders the shared KanaBlitz_Index hub (solo + multiplayer mode picker).
        public IActionResult Index()
        {
            return View("~/Views/KanaBattle/KanaBlitz_Index.cshtml");
        }

        // ── 2. Multiplayer Lobby / Setup ──
        // GET /KanaBattle/MainView
        // Renders the KanaBattle_View.cshtml with lobby setup for multiplayer.
        // Players can choose difficulty, alphabet, and enter names before battle.
        public IActionResult MainView()
        {
            // Default model for the lobby
            var model = new KanaBattleModel
            {
                Difficulty = "normal",
                Script = "hiragana",
                PlayerName = "Player 1"
            };

            ViewBag.Multiplayer = true;
            ViewBag.Player2Name = "Player 2";

            return View("~/Views/KanaBattle/KanaBattle_View.cshtml", model);
        }

        // ── 3. Battle Arena ──
        // GET /KanaBattle/Battle?difficulty=normal&script=hiragana
        //
        // Builds a KanaBattleModel and renders KanaBattle_View.cshtml.
        // The view fetches words client-side via:
        //   GET /KanaBlitz/GetWords?difficulty={difficulty}&alphabet={script}
        [HttpGet]
        public IActionResult Battle(
            string difficulty = "normal",
            string script = "hiragana",
            string playerName = "Player 1",
            string player2Name = "Player 2")
        {
            // Normalise to lower-case — DbInitializer seeds with lower-case values
            difficulty = difficulty?.ToLowerInvariant() ?? "normal";
            script = script?.ToLowerInvariant() ?? "hiragana";

            // Validate
            var validDiffs = new[] { "easy", "normal", "hard", "insanity" };
            var validScripts = new[] { "hiragana", "katakana", "dakuten", "mixed" };

            if (!Array.Exists(validDiffs, d => d == difficulty)) difficulty = "normal";
            if (!Array.Exists(validScripts, s => s == script)) script = "hiragana";

            var model = new KanaBattleModel
            {
                Difficulty = difficulty,
                Script = script,
                PlayerName = playerName ?? "Player 1"
            };

            ViewBag.Multiplayer = true;
            ViewBag.Player2Name = player2Name ?? "Player 2";

            return View("~/Views/KanaBattle/KanaBattle_View.cshtml", model);
        }
    }
}

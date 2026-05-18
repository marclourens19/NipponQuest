using Microsoft.AspNetCore.Mvc;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    // Routes to /KanaBattle/...
    [Route("[controller]/[action]")]
    public class KanaBattleController : Controller
    {
        // ── 1. Hub / Mode-Select ───────────────────────────────────────
        // GET /KanaBattle/Index
        // Renders the shared KanaBlitz_Index hub (solo + multiplayer mode picker).
        public IActionResult Index()
        {
            return View("~/Views/KanaBattle/KanaBlitz_Index.cshtml");
        }

        // ── 2. Multiplayer Lobby / Setup ───────────────────────────────
        // GET /KanaBattle/MainView
        // Renders the KanaBlitz_View (multiplayer setup / lobby page).
        public IActionResult MainView()
        {
            return View("~/Views/KanaBattle/KanaBlitz_View.cshtml");
        }

        // ── 3. Battle Arena ────────────────────────────────────────────
        // GET /KanaBattle/Battle?difficulty=normal&script=hiragana
        //
        // Builds a KanaBattleModel and renders KanaBattle_View.cshtml.
        // The view fetches words client-side via:
        //   GET /KanaBlitz/GetWords?difficulty={difficulty}&script={script}
        // (KanaBlitzController.GetWords shares this endpoint.)
        //
        // FIX: normalise & validate inputs before building the model so that
        // the shared GetWords endpoint is never called with invalid params.
        [HttpGet]
        public IActionResult Battle(
            string difficulty = "normal",
            string script = "hiragana",
            string playerName = "Player 1")
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

            return View("~/Views/KanaBattle/KanaBattle_View.cshtml", model);
        }
    }
}

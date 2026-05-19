using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;

namespace NipponQuest.Controllers
{
    [Authorize]
    public class KanaBlitzController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        private const double HardUnlockThreshold = 0.90;
        private const int HardCorrectFloor = 30;
        private const double InsanityUnlockThreshold = 0.95;
        private const int InsanityCorrectFloor = 25;
        private const int InsanityRequiredLevel = 15;
        private const int MinAttemptsForUnlock = 8;

        private static readonly string[] GatedAlphabets = new[] { "hiragana", "katakana", "dakuten", "mixed" };

        public KanaBlitzController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ── Mode Selection Hub ──
        // GET /KanaBlitz/Index or /KanaBlitz
        public IActionResult Index()
        {
            return View("~/Views/KanaBattle/KanaBlitz_Index.cshtml");
        }

        // ── Solo Game Arena ──
        // GET /KanaBlitz/Play
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Play()
        {
            return View("~/Views/KanaBattle/KanaBlitz_View.cshtml");
        }

        // ── Get Acknowledgements ──
        [HttpGet]
        public async Task<IActionResult> GetAcknowledgements()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var stats = ParseAccuracy(user.BlitzAccuracyJson);
            return Json(new
            {
                rules = stats.GetValueOrDefault("ack:kanablitz_rules", 0.0) >= 1.0,
                insanity = stats.GetValueOrDefault("ack:insanity_warning", 0.0) >= 1.0
            });
        }

        // ── Accept Acknowledgement ──
        [HttpPost]
        public async Task<IActionResult> AcceptAcknowledgement([FromForm] AcknowledgementDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var key = (dto?.Key ?? "").Trim().ToLowerInvariant();
            if (key != "rules" && key != "insanity")
                return BadRequest(new { error = "Invalid acknowledgement key." });

            var stats = ParseAccuracy(user.BlitzAccuracyJson);
            stats[key == "rules" ? "ack:kanablitz_rules" : "ack:insanity_warning"] = 1.0;
            user.BlitzAccuracyJson = JsonSerializer.Serialize(stats);

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, key });
        }

        // ── Get Unlocks ──
        [HttpGet]
        public async Task<IActionResult> GetUnlocks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var stats = ParseAccuracy(user.BlitzAccuracyJson);
            int level = ResolveLevel(user);

            var hardAcc = GatedAlphabets.ToDictionary(a => a, a => stats.GetValueOrDefault($"normal:{a}", 0.0));
            var hardMax = GatedAlphabets.ToDictionary(a => a, a => (int)stats.GetValueOrDefault($"normal:{a}:max", 0.0));

            bool hardUnlocked = GatedAlphabets.All(a =>
                hardAcc[a] >= HardUnlockThreshold && hardMax[a] >= HardCorrectFloor);

            var insAcc = GatedAlphabets.ToDictionary(a => a, a => stats.GetValueOrDefault($"hard:{a}", 0.0));
            var insMax = GatedAlphabets.ToDictionary(a => a, a => (int)stats.GetValueOrDefault($"hard:{a}:max", 0.0));

            bool insanityHardCleared = GatedAlphabets.All(a =>
                insAcc[a] >= InsanityUnlockThreshold && insMax[a] >= InsanityCorrectFloor);
            bool insanityUnlocked = insanityHardCleared && level >= InsanityRequiredLevel;

            return Json(new
            {
                level,
                hard = new
                {
                    unlocked = hardUnlocked,
                    threshold = HardUnlockThreshold,
                    correctFloor = HardCorrectFloor,
                    progress = hardAcc,
                    progressMax = hardMax
                },
                insanity = new
                {
                    unlocked = insanityUnlocked,
                    threshold = InsanityUnlockThreshold,
                    correctFloor = InsanityCorrectFloor,
                    requiredLevel = InsanityRequiredLevel,
                    levelMet = level >= InsanityRequiredLevel,
                    hardCleared = insanityHardCleared,
                    progress = insAcc,
                    progressMax = insMax
                }
            });
        }

        // ── Get Arena Payload (word counts + unlocks) ──
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetArenaPayload(string alphabet, string difficulty)
        {
            try
            {
                if (string.IsNullOrEmpty(alphabet) || string.IsNullOrEmpty(difficulty))
                    return BadRequest(new { error = "Missing alphabet or difficulty parameter." });

                var diff = difficulty.ToLower();
                var alpha = alphabet.ToLower();

                var user = await _userManager.GetUserAsync(User);

                // Build base query
                IQueryable<KanaWord> query = _context.KanaWords.AsNoTracking();

                // Apply difficulty filter
                query = query.Where(w => w.DifficultyLevel == diff);

                // Apply alphabet filter - "mixed" means all alphabets
                if (alpha != "mixed")
                {
                    query = query.Where(w => w.Alphabet == alpha);
                }

                var words = await query.ToListAsync();

                // Calculate word count per alphabet for display
                var countsByAlpha = new Dictionary<string, int>();
                foreach (var a in GatedAlphabets)
                {
                    var q = _context.KanaWords.AsNoTracking().Where(w => w.DifficultyLevel == diff);
                    if (a != "mixed")
                        q = q.Where(w => w.Alphabet == a);
                    countsByAlpha[a] = await q.CountAsync();
                }

                // Check locks if user is authenticated
                bool hardLocked = true;
                bool insanityLocked = true;

                if (user != null)
                {
                    var stats = ParseAccuracy(user.BlitzAccuracyJson);
                    int level = ResolveLevel(user);

                    var hardAcc = GatedAlphabets.ToDictionary(a => a, a => stats.GetValueOrDefault($"normal:{a}", 0.0));
                    var hardMax = GatedAlphabets.ToDictionary(a => a, a => (int)stats.GetValueOrDefault($"normal:{a}:max", 0.0));
                    hardLocked = !GatedAlphabets.All(a => hardAcc[a] >= HardUnlockThreshold && hardMax[a] >= HardCorrectFloor);

                    var insAcc = GatedAlphabets.ToDictionary(a => a, a => stats.GetValueOrDefault($"hard:{a}", 0.0));
                    var insMax = GatedAlphabets.ToDictionary(a => a, a => (int)stats.GetValueOrDefault($"hard:{a}:max", 0.0));
                    bool insHardCleared = GatedAlphabets.All(a => insAcc[a] >= InsanityUnlockThreshold && insMax[a] >= InsanityCorrectFloor);
                    insanityLocked = !insHardCleared || level < InsanityRequiredLevel;
                }

                return Json(new
                {
                    alphabet = alpha,
                    difficulty = diff,
                    totalWords = words.Count,
                    countsByAlpha,
                    locks = new { hard = hardLocked, insanity = insanityLocked }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load arena data.", detail = ex.Message });
            }
        }

        // ── Get Words (JSON API) ──
        // GET /KanaBlitz/GetWords?difficulty=normal&alphabet=hiragana
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWords(string difficulty, string alphabet)
        {
            try
            {
                if (string.IsNullOrEmpty(difficulty) || string.IsNullOrEmpty(alphabet))
                    return BadRequest(new { error = "Missing difficulty or alphabet parameter." });

                var diff = difficulty.ToLower();
                var alpha = alphabet.ToLower();

                // Validate difficulty
                var validDiffs = new[] { "easy", "normal", "hard", "insanity" };
                var validAlphas = new[] { "hiragana", "katakana", "dakuten", "mixed" };

                if (!validDiffs.Contains(diff))
                    return BadRequest(new { error = $"Invalid difficulty: {diff}. Valid: {string.Join(", ", validDiffs)}" });

                if (!validAlphas.Contains(alpha))
                    return BadRequest(new { error = $"Invalid alphabet: {alpha}. Valid: {string.Join(", ", validAlphas)}" });

                // Build query
                IQueryable<KanaWord> query = _context.KanaWords.AsNoTracking();

                query = query.Where(w => w.DifficultyLevel == diff);

                if (alpha != "mixed")
                {
                    query = query.Where(w => w.Alphabet == alpha);
                }

                var words = await query.ToListAsync();

                if (words.Count == 0)
                {
                    // Fallback: try broader search (e.g., for dakuten which may have fewer words)
                    var fallbackQuery = _context.KanaWords.AsNoTracking()
                        .Where(w => w.DifficultyLevel == diff);

                    var fallbackWords = await fallbackQuery.ToListAsync();

                    if (fallbackWords.Count > 0)
                    {
                        // Return what we have with a warning
                        return Json(fallbackWords.Select(w => new
                        {
                            w.Id,
                            w.WordKana,
                            w.WordRomaji,
                            w.MeaningEnglish,
                            w.Alphabet,
                            w.DifficultyLevel,
                            w.MissingKana,
                            w.CategoryTag
                        }));
                    }

                    return Json(new List<object>());
                }

                // Return word data
                return Json(words.Select(w => new
                {
                    w.Id,
                    w.WordKana,
                    w.WordRomaji,
                    w.MeaningEnglish,
                    w.Alphabet,
                    w.DifficultyLevel,
                    w.MissingKana,
                    w.CategoryTag
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve words.", detail = ex.Message });
            }
        }

        // ── Submit Score ──
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmissionDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Still return success for anonymous users
                    return Json(new { success = true, xpEarned = dto?.XpEarned ?? 0 });
                }

                if (dto == null) return BadRequest(new { error = "Invalid score data." });

                var diff = (dto.Difficulty ?? "normal").ToLower();
                var alpha = (dto.Alphabet ?? "hiragana").ToLower();

                // Update accuracy tracking
                var stats = ParseAccuracy(user.BlitzAccuracyJson);
                var key = $"{diff}:{alpha}";
                var maxKey = $"{diff}:{alpha}:max";

                var total = dto.Total > 0 ? dto.Total : 1;
                var accuracy = (double)dto.Correct / total;
                var currentAcc = stats.GetValueOrDefault(key, 0.0);
                var currentMax = stats.GetValueOrDefault(maxKey, 0.0);

                // Update running accuracy (weighted average)
                stats[key] = currentAcc == 0 ? accuracy : (currentAcc * 0.7 + accuracy * 0.3);
                stats[maxKey] = Math.Max(currentMax, dto.Correct);

                user.BlitzAccuracyJson = JsonSerializer.Serialize(stats);

                // Update highest blitz score
                if (dto.Score > user.HighestBlitzScore)
                    user.HighestBlitzScore = dto.Score;

                // Award XP
                int xpEarned = dto.XpEarned ?? (int)(dto.Score * 0.5 + dto.Correct * 5);
                user.AddExperience(xpEarned);

                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, xpEarned });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ── Helper Methods ──
        private static Dictionary<string, double> ParseAccuracy(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, double>();
            try { return JsonSerializer.Deserialize<Dictionary<string, double>>(json) ?? new(); }
            catch { return new Dictionary<string, double>(); }
        }

        private static int ResolveLevel(ApplicationUser user) => user?.Level ?? 1;
    }

    // DTOs
    public class AcknowledgementDto
    {
        public string? Key { get; set; }
    }

    public class ScoreSubmissionDto
    {
        public int Score { get; set; }
        public string? Difficulty { get; set; }
        public string? Alphabet { get; set; }
        public int Correct { get; set; }
        public int Total { get; set; }
        public int MaxCombo { get; set; }
        public int? XpEarned { get; set; }
    }
}

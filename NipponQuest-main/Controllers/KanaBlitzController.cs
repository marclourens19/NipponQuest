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

        // ── Unlock gates ──
        // HARD     : 90% accuracy AND 30+ correct words on every Normal script
        // INSANITY : 95% accuracy AND 25+ correct words on every Hard script + Account Level 15
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

        public IActionResult Index() => View();

        // ─────────────────────────────────────────────────────────────
        //  ONE-TIME ACKNOWLEDGEMENTS
        //  Stored inside BlitzAccuracyJson so no schema change is required.
        // ─────────────────────────────────────────────────────────────
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

        [HttpPost]
        public async Task<IActionResult> AcceptAcknowledgement([FromBody] AcknowledgementDto dto)
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

        // ─────────────────────────────────────────────────────────────
        //  UNLOCKS
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        //  ARENA PAYLOAD
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetArenaPayload(string alphabet, string difficulty)
        {
            try
            {
                if (string.IsNullOrEmpty(alphabet) || string.IsNullOrEmpty(difficulty))
                    return BadRequest(new { error = "Missing alphabet or difficulty parameter." });

                var diff = difficulty.ToLower();
                var alpha = alphabet.ToLower();

                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.GamerTag != "NQDev" && (diff == "hard" || diff == "insanity"))
                {
                    var stats = ParseAccuracy(user.BlitzAccuracyJson);
                    int level = ResolveLevel(user);

                    if (diff == "hard")
                    {
                        bool ok = GatedAlphabets.All(a =>
                            stats.GetValueOrDefault($"normal:{a}", 0.0) >= HardUnlockThreshold &&
                            stats.GetValueOrDefault($"normal:{a}:max", 0.0) >= HardCorrectFloor);
                        if (!ok)
                            return StatusCode(403, new { error = $"Hard mode is locked. Reach {(int)(HardUnlockThreshold * 100)}% accuracy AND {HardCorrectFloor}+ correct words in a single Normal run on every script." });
                    }
                    else
                    {
                        bool clearedHard = GatedAlphabets.All(a =>
                            stats.GetValueOrDefault($"hard:{a}", 0.0) >= InsanityUnlockThreshold &&
                            stats.GetValueOrDefault($"hard:{a}:max", 0.0) >= InsanityCorrectFloor);
                        if (!clearedHard || level < InsanityRequiredLevel)
                            return StatusCode(403, new { error = $"Insanity is locked. Reach {(int)(InsanityUnlockThreshold * 100)}% accuracy AND {InsanityCorrectFloor}+ correct words in a single Hard run on every script, plus Account Level {InsanityRequiredLevel}." });
                    }
                }

                // ── Build query for the requested difficulty ──
                var query = _context.KanaWords.AsNoTracking()
                    .Where(w => w.DifficultyLevel == diff);

                // ── Filter by alphabet ──
                if (alpha == "mixed")
                {
                    // Mixed pulls from hiragana, katakana, dakuten, AND mixed alphabet entries
                    query = query.Where(w => w.Alphabet == "hiragana"
                                          || w.Alphabet == "katakana"
                                          || w.Alphabet == "dakuten"
                                          || w.Alphabet == "mixed");
                }
                else
                {
                    // For specific alphabets, match ONLY that exact alphabet
                    // This ensures hiragana -> hiragana kana only, katakana -> katakana kana only, etc.
                    // For insanity mode: include "mixed" only for the mixed script selection,
                    // but for hiragana/katakana/dakuten-specific runs, keep it pure.
                    if (diff == "insanity")
                    {
                        // Insanity allows mixed kanji entries alongside specific alphabet entries
                        query = query.Where(w => w.Alphabet == alpha
                                              || w.Alphabet == "mixed"
                                              || string.IsNullOrEmpty(w.Alphabet));
                    }
                    else
                    {
                        // Normal/Hard: strict alphabet match only
                        query = query.Where(w => w.Alphabet == alpha
                                              || string.IsNullOrEmpty(w.Alphabet));
                    }
                }

                var rawWords = await query.ToListAsync();
                var rng = new Random();

                // ── If database is empty, generate a small local pool ──
                if (rawWords.Count == 0)
                {
                    var localPool = GenerateLocalFallback(diff, alpha, rng);
                    return Json(new { alphabet = alpha, words = localPool, distractors = new List<string>() });
                }

                // ── Process database words into arena payload ──
                var payload = new List<object>();
                var distractorSet = new HashSet<string>();

                // Build a kanji distractor pool for insanity mode
                var kanjiDistractors = diff == "insanity"
                    ? rawWords
                        .Where(w => (w.WordKana ?? "").Contains('(') && (w.WordKana ?? "").Contains(')'))
                        .Select(w => { int o = (w.WordKana ?? "").IndexOf('('); return w.WordKana!.Substring(0, o).Trim(); })
                        .Distinct()
                        .ToList()
                    : new List<string>();

                // Build a furigana distractor pool for insanity mode
                var furiganaDistractors = diff == "insanity"
                    ? rawWords
                        .Where(w => (w.WordKana ?? "").Contains('(') && (w.WordKana ?? "").Contains(')'))
                        .Select(w => { int o = (w.WordKana ?? "").IndexOf('('); int c = (w.WordKana ?? "").IndexOf(')'); return w.WordKana!.Substring(o + 1, c - o - 1).Trim(); })
                        .Distinct()
                        .ToList()
                    : new List<string>();

                foreach (var w in rawWords)
                {
                    string missing;
                    string display;
                    string questionType = "kana"; // "kana", "furigana_blank", "kanji_from_furigana", "furigana_from_kanji"
                    string fullKana = w.WordKana ?? "";

                    if (diff == "easy")
                    {
                        display = (w.WordRomaji ?? "").ToUpper();
                        missing = fullKana;
                        questionType = "easy";
                    }
                    else if (diff == "insanity" && fullKana.Contains('(') && fullKana.Contains(')'))
                    {
                        int open = fullKana.IndexOf('(');
                        int close = fullKana.IndexOf(')');
                        string kanji = fullKana.Substring(0, open).Trim();
                        string furigana = fullKana.Substring(open + 1, close - open - 1).Trim();

                        // 50/50: either blank a furigana syllable OR ask user to identify the kanji
                        if (rng.Next(2) == 0)
                        {
                            // Mode A: Show kanji, display furigana with a blank — answer is the missing syllable
                            (missing, display) = BuildRandomMissing(fullKana, diff, rng);
                            questionType = "furigana_blank";
                        }
                        else
                        {
                            // Mode B: Show full furigana reading, ask which kanji it matches
                            missing = kanji;
                            display = $"<div class=\"kanji-stack\">" +
                                      $"<div class=\"kanji-hint\">{w.MeaningEnglish ?? ""}</div>" +
                                      $"<div class=\"kanji-top\" style=\"font-size:1.8rem;color:#475569;letter-spacing:4px;\">{furigana}</div>" +
                                      $"<p style=\"font-size:0.65rem;color:#94a3b8;font-weight:600;margin-top:0;\">Select the matching Kanji</p>" +
                                      $"</div>";
                            questionType = "kanji_from_furigana";
                        }
                    }
                    else
                    {
                        (missing, display) = BuildRandomMissing(fullKana, diff, rng);
                        questionType = "kana";
                    }

                    if (!string.IsNullOrEmpty(missing))
                        distractorSet.Add(missing);

                    payload.Add(new
                    {
                        id = w.Id,
                        kana = fullKana,
                        romaji = w.WordRomaji ?? "",
                        meaning = w.MeaningEnglish ?? "",
                        category = w.CategoryTag ?? "",
                        missing,
                        display,
                        questionType,
                        alphabet = w.Alphabet ?? alpha
                    });
                }

                // Shuffle and take a reasonable batch (50 words per run)
                var shuffled = payload.OrderBy(_ => Guid.NewGuid()).Take(50).ToList();

                return Json(new
                {
                    alphabet = alpha,
                    words = shuffled,
                    distractors = distractorSet.ToList(),
                    kanjiDistractors,
                    furiganaDistractors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Arena payload failed.",
                    detail = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  RANDOM-MISSING BUILDER
        //  Splits a word into syllables (treating ゃゅょっッー as part of
        //  the previous kana) and blanks ONE random syllable. Works for
        //  both plain kana words and "Kanji (furigana)" insanity entries.
        // ─────────────────────────────────────────────────────────────
        private static (string missing, string displayHtml) BuildRandomMissing(string wordKana, string difficulty, Random rng)
        {
            if (string.IsNullOrEmpty(wordKana)) return ("?", "<span class='blank'>?</span>");

            // INSANITY format: "漢字 (ふりがな)" — blank inside the furigana
            if (wordKana.Contains('(') && wordKana.Contains(')'))
            {
                int open = wordKana.IndexOf('(');
                int close = wordKana.IndexOf(')');
                string kanji = wordKana.Substring(0, open).Trim();
                string furigana = wordKana.Substring(open + 1, close - open - 1).Trim();

                var sylls = SplitSyllables(furigana);
                if (sylls.Count == 0)
                {
                    return ("?", $"<div class=\"kanji-stack\"><div class=\"kanji-top\">{kanji}</div><div class=\"kana-bottom\"><span class='blank'>?</span></div></div>");
                }

                int idx = rng.Next(sylls.Count);
                string missingChar = sylls[idx];

                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < sylls.Count; i++)
                {
                    if (i == idx) sb.Append($"<span class='blank'>?</span>");
                    else sb.Append($"<span class='char'>{sylls[i]}</span>");
                }

                string html = $"<div class=\"kanji-stack\">" +
                              $"<div class=\"kanji-top\">{kanji}</div>" +
                              $"<div class=\"kana-bottom\">{sb}</div>" +
                              $"</div>";
                return (missingChar, html);
            }

            // NORMAL / HARD plain kana — wrap ALL chars in spans for proper display
            var s = SplitSyllables(wordKana);
            if (s.Count == 0) return ("?", "<span class='blank'>?</span>");

            int p = rng.Next(s.Count);
            string miss = s[p];

            var outBuf = new System.Text.StringBuilder();
            for (int i = 0; i < s.Count; i++)
            {
                if (i == p) outBuf.Append($"<span class='blank'>?</span>");
                else outBuf.Append($"<span class='char'>{s[i]}</span>");
            }
            return (miss, outBuf.ToString());
        }

        private static List<string> SplitSyllables(string s)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(s)) return list;

            var smallMods = new HashSet<char> { 'ゃ', 'ゅ', 'ょ', 'ャ', 'ュ', 'ョ', 'っ', 'ッ', 'ー' };

            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == ' ' || c == '(' || c == ')') { i++; continue; }
                string syll = c.ToString();
                if (i + 1 < s.Length && smallMods.Contains(s[i + 1]))
                {
                    syll += s[i + 1];
                    i += 2;
                }
                else
                {
                    i += 1;
                }
                list.Add(syll);
            }
            return list;
        }

        // ─────────────────────────────────────────────────────────────
        //  LOCAL FALLBACK — generates a small pool when DB is empty
        // ─────────────────────────────────────────────────────────────
        private static List<object> GenerateLocalFallback(string difficulty, string alphabet, Random rng)
        {
            var pool = new List<(string kana, string romaji, string meaning)>();

            if (difficulty == "easy")
            {
                pool.Add(("あ", "A", "Pick the kana"));
                pool.Add(("い", "I", "Pick the kana"));
                pool.Add(("う", "U", "Pick the kana"));
                pool.Add(("え", "E", "Pick the kana"));
                pool.Add(("お", "O", "Pick the kana"));
                pool.Add(("か", "KA", "Pick the kana"));
                pool.Add(("き", "KI", "Pick the kana"));
                pool.Add(("く", "KU", "Pick the kana"));
                pool.Add(("け", "KE", "Pick the kana"));
                pool.Add(("こ", "KO", "Pick the kana"));
                pool.Add(("さ", "SA", "Pick the kana"));
                pool.Add(("し", "SHI", "Pick the kana"));
                pool.Add(("す", "SU", "Pick the kana"));
                pool.Add(("せ", "SE", "Pick the kana"));
                pool.Add(("そ", "SO", "Pick the kana"));
            }
            else if (difficulty == "normal" || difficulty == "hard")
            {
                pool.Add(("ねこ", "neko", "Cat"));
                pool.Add(("いぬ", "inu", "Dog"));
                pool.Add(("とり", "tori", "Bird"));
                pool.Add(("さかな", "sakana", "Fish"));
                pool.Add(("うま", "uma", "Horse"));
                pool.Add(("くま", "kuma", "Bear"));
                pool.Add(("やま", "yama", "Mountain"));
                pool.Add(("かわ", "kawa", "River"));
                pool.Add(("みず", "mizu", "Water"));
                pool.Add(("そら", "sora", "Sky"));
                pool.Add(("はな", "hana", "Flower"));
                pool.Add(("つき", "tsuki", "Moon"));
                pool.Add(("ほし", "hoshi", "Star"));
                pool.Add(("うみ", "umi", "Sea"));
                pool.Add(("かぜ", "kaze", "Wind"));
                pool.Add(("あめ", "ame", "Rain"));
                pool.Add(("ゆき", "yuki", "Snow"));
                pool.Add(("いえ", "ie", "House"));
                pool.Add(("ほん", "hon", "Book"));
                pool.Add(("かばん", "kaban", "Bag"));
                pool.Add(("とけい", "tokei", "Watch"));
                pool.Add(("かさ", "kasa", "Umbrella"));
                pool.Add(("くつ", "kutsu", "Shoes"));
                pool.Add(("ふく", "fuku", "Clothes"));
                pool.Add(("たべる", "taberu", "To Eat"));
                pool.Add(("のむ", "nomu", "To Drink"));
                pool.Add(("みる", "miru", "To See"));
                pool.Add(("いく", "iku", "To Go"));
                pool.Add(("くる", "kuru", "To Come"));
                pool.Add(("よむ", "yomu", "To Read"));
                pool.Add(("あさ", "asa", "Morning"));
                pool.Add(("ひる", "hiru", "Noon"));
                pool.Add(("よる", "yoru", "Night"));
                pool.Add(("はる", "haru", "Spring"));
                pool.Add(("なつ", "natsu", "Summer"));
                pool.Add(("あき", "aki", "Autumn"));
                pool.Add(("ふゆ", "fuyu", "Winter"));
                pool.Add(("あか", "aka", "Red"));
                pool.Add(("あお", "ao", "Blue"));
                pool.Add(("しろ", "shiro", "White"));
                pool.Add(("くろ", "kuro", "Black"));
            }
            else // insanity
            {
                pool.Add(("学校 (がっこう)", "gakkou", "School"));
                pool.Add(("先生 (せんせい)", "sensei", "Teacher"));
                pool.Add(("学生 (がくせい)", "gakusei", "Student"));
                pool.Add(("家族 (かぞく)", "kazoku", "Family"));
                pool.Add(("時間 (じかん)", "jikan", "Time"));
                pool.Add(("天気 (てんき)", "tenki", "Weather"));
                pool.Add(("電車 (でんしゃ)", "densha", "Train"));
                pool.Add(("会社 (かいしゃ)", "kaisha", "Company"));
                pool.Add(("猫 (ねこ)", "neko", "Cat"));
                pool.Add(("犬 (いぬ)", "inu", "Dog"));
                pool.Add(("山 (やま)", "yama", "Mountain"));
                pool.Add(("川 (かわ)", "kawa", "River"));
                pool.Add(("花 (はな)", "hana", "Flower"));
                pool.Add(("月 (つき)", "tsuki", "Moon"));
                pool.Add(("海 (うみ)", "umi", "Sea"));
                pool.Add(("心 (こころ)", "kokoro", "Heart"));
                pool.Add(("夢 (ゆめ)", "yume", "Dream"));
                pool.Add(("力 (ちから)", "chikara", "Power"));
                pool.Add(("道 (みち)", "michi", "Road"));
                pool.Add(("水 (みず)", "mizu", "Water"));
            }

            var result = new List<object>();
            foreach (var (kana, romaji, meaning) in pool)
            {
                string missing;
                string display;

                if (difficulty == "easy")
                {
                    display = romaji.ToUpper();
                    missing = kana;
                }
                else
                {
                    (missing, display) = BuildRandomMissing(kana, difficulty, rng);
                }

                result.Add(new
                {
                    id = 0,
                    kana,
                    romaji,
                    meaning,
                    category = "Local",
                    missing,
                    display
                });
            }

            return result.OrderBy(_ => Guid.NewGuid()).Take(50).ToList();
        }

        // ─────────────────────────────────────────────────────────────
        //  SUBMIT SCORE
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> SubmitScore([FromBody] BlitzResultDto result)
        {
            try
            {
                if (result == null) return BadRequest(new { error = "Empty payload." });

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                string diffKey = (result.Difficulty ?? "").ToLower();
                string alphaKey = (result.Alphabet ?? "").ToLower();

                // ── XP / GOLD computation ──
                const double baseExp = 3.0;
                const double baseGold = 0.8;

                double diffMult = diffKey switch
                {
                    "easy" => 1.0,
                    "normal" => 2.0,
                    "hard" => 4.0,
                    "insanity" => 8.0,
                    _ => 1.0
                };

                double comboMult = 1.0 + Math.Min(result.MaxCombo, 30) * 0.04;

                int totalAttempts = result.CorrectAnswers + result.Mistakes;
                double accuracy = totalAttempts > 0 ? (double)result.CorrectAnswers / totalAttempts : 0;
                double accuracyMult = accuracy switch
                {
                    >= 0.95 => 1.30,
                    >= 0.85 => 1.10,
                    >= 0.70 => 1.00,
                    _ => 0.90
                };

                double rawExp = result.CorrectAnswers * baseExp * diffMult * comboMult * accuracyMult;
                double rawGold = result.CorrectAnswers * baseGold * diffMult * comboMult * accuracyMult;

                int perfectExpBonus = 0;
                int perfectGoldBonus = 0;
                if (result.Mistakes == 0 && result.CorrectAnswers >= 5)
                {
                    perfectExpBonus = (int)(20 * diffMult);
                    perfectGoldBonus = (int)(8 * diffMult);
                }

                int mistakePenalty = Math.Min(result.Mistakes * 2, (int)(rawExp * 0.3));

                int expEarned = Math.Max(0, (int)Math.Round(rawExp) - mistakePenalty + perfectExpBonus);
                int goldEarned = Math.Max(0, (int)Math.Round(rawGold) + perfectGoldBonus);

                user.AddExperience(expEarned);
                user.Gold += goldEarned;
                user.LessonsCompleted += 1;

                _context.RewardLedgers.Add(new RewardLedger
                {
                    ApplicationUserId = user.Id,
                    ExpDelta = expEarned,
                    GoldDelta = goldEarned,
                    Source = "kanablitz"
                });

                // ── BlitzAccuracyJson update ──
                if (totalAttempts >= 5)
                {
                    var stats = ParseAccuracy(user.BlitzAccuracyJson);
                    string accK = $"{diffKey}:{alphaKey}";
                    string maxK = $"{diffKey}:{alphaKey}:max";

                    if (totalAttempts >= MinAttemptsForUnlock && accuracy > stats.GetValueOrDefault(accK, 0.0))
                        stats[accK] = accuracy;

                    if (result.CorrectAnswers > stats.GetValueOrDefault(maxK, 0.0))
                        stats[maxK] = result.CorrectAnswers;

                    user.BlitzAccuracyJson = JsonSerializer.Serialize(stats);
                }

                // ── PERSONAL BEST + LEADERBOARD DELTA ──
                bool isNewPB = false;
                int rankBefore = 0;
                int rankAfter = 0;
                int pbCorrect = 0;
                int pbPoints = 0;

                if (!string.IsNullOrEmpty(diffKey) && !string.IsNullOrEmpty(alphaKey))
                {
                    var pb = await _context.BlitzPersonalBests
                        .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id
                                              && p.Difficulty == diffKey
                                              && p.Alphabet == alphaKey);

                    int previousBestCorrect = pb?.BestCorrect ?? 0;

                    rankBefore = await _context.BlitzPersonalBests
                        .Where(p => p.Difficulty == diffKey && p.Alphabet == alphaKey)
                        .CountAsync(p => p.BestCorrect > previousBestCorrect) + 1;

                    if (pb == null)
                    {
                        pb = new BlitzPersonalBest
                        {
                            ApplicationUserId = user.Id,
                            Difficulty = diffKey,
                            Alphabet = alphaKey
                        };
                        _context.BlitzPersonalBests.Add(pb);
                    }

                    if (result.CorrectAnswers > pb.BestCorrect) { pb.BestCorrect = result.CorrectAnswers; isNewPB = true; }
                    if (result.Points > pb.BestPoints) { pb.BestPoints = result.Points; isNewPB = true; }
                    if (result.MaxCombo > pb.BestCombo) { pb.BestCombo = result.MaxCombo; }
                    if (totalAttempts >= MinAttemptsForUnlock && accuracy > pb.BestAccuracy)
                        pb.BestAccuracy = accuracy;
                    pb.UpdatedAt = DateTime.UtcNow;

                    pbCorrect = pb.BestCorrect;
                    pbPoints = pb.BestPoints;

                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();

                    rankAfter = await _context.BlitzPersonalBests
                        .Where(p => p.Difficulty == diffKey && p.Alphabet == alphaKey)
                        .CountAsync(p => p.BestCorrect > pb.BestCorrect) + 1;
                }
                else
                {
                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    exp = expEarned,
                    gold = goldEarned,
                    points = result.Points,
                    correct = result.CorrectAnswers,
                    accuracy = Math.Round(accuracy * 100),
                    maxCombo = result.MaxCombo,
                    perfectBonus = perfectExpBonus > 0,

                    isNewPB,
                    pbCorrect,
                    pbPoints,
                    rankBefore,
                    rankAfter,
                    rankDelta = rankBefore - rankAfter
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Score submission failed.",
                    detail = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────
        private static Dictionary<string, double> ParseAccuracy(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, double>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, double>>(json)
                       ?? new Dictionary<string, double>();
            }
            catch
            {
                return new Dictionary<string, double>();
            }
        }

        private bool ValidateAiWord(KanaWord word)
        {
            // Basic sanity: romaji length matches kana length (rough check)
            if (string.IsNullOrWhiteSpace(word.WordKana) ||
                string.IsNullOrWhiteSpace(word.WordRomaji) ||
                string.IsNullOrWhiteSpace(word.MissingKana))
                return false;

            // Ensure the missing fragment actually exists in the full kana string
            if (!word.WordKana.Contains(word.MissingKana))
                return false;

            return true;
        }

        private static int ResolveLevel(ApplicationUser user)
        {
            var t = user.GetType();
            foreach (var name in new[] { "Level", "CurrentLevel", "AccountLevel", "PlayerLevel" })
            {
                var p = t.GetProperty(name);
                if (p != null && p.PropertyType == typeof(int))
                    return (int)(p.GetValue(user) ?? 0);
            }
            var xpProp = t.GetProperty("TotalEXP");
            if (xpProp != null && xpProp.PropertyType == typeof(int))
                return Math.Max(1, ((int)(xpProp.GetValue(user) ?? 0)) / 1000 + 1);
            return 1;
        }
    }

    public class BlitzResultDto
    {
        public string Alphabet { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public int CorrectAnswers { get; set; }
        public int Mistakes { get; set; }
        public int MaxCombo { get; set; }
        public int Points { get; set; }
        public int DurationSeconds { get; set; }
    }

    public class AcknowledgementDto
    {
        public string Key { get; set; } = "";
    }
}
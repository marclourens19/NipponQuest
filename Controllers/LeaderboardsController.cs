using System.Linq;
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
    public class LeaderboardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] AllowedAlphabets =
            new[] { "hiragana", "katakana", "dakuten", "mixed" };

        public LeaderboardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Blitz() => View();

        [HttpGet]
        public async Task<IActionResult> GetBlitzBoard(string alphabet = "hiragana", int top = 50)
        {
            alphabet = (alphabet ?? "").ToLower();
            if (!AllowedAlphabets.Contains(alphabet))
                return BadRequest(new { error = "Unknown alphabet." });

            top = System.Math.Clamp(top, 10, 200);

            var rows = await _context.BlitzPersonalBests
                .AsNoTracking()
                .Where(p => p.Difficulty == "normal" && p.Alphabet == alphabet)
                .OrderByDescending(p => p.BestCorrect)
                .ThenByDescending(p => p.BestPoints)
                .ThenBy(p => p.UpdatedAt)
                .Take(top)
                .Select(p => new
                {
                    userId = p.ApplicationUserId,
                    name = (p.User != null && !string.IsNullOrEmpty(p.User.GamerTag))
                                       ? p.User.GamerTag
                                       : (p.User != null ? p.User.UserName : "Anonymous"),
                    bestCorrect = p.BestCorrect,
                    bestPoints = p.BestPoints,
                    bestCombo = p.BestCombo,
                    bestAccuracy = p.BestAccuracy,
                    updatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var me = await _userManager.GetUserAsync(User);
            object? selfRow = null;
            int selfRank = 0;

            if (me != null)
            {
                var myPb = await _context.BlitzPersonalBests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == me.Id
                                           && p.Difficulty == "normal"
                                           && p.Alphabet == alphabet);

                if (myPb != null)
                {
                    selfRank = await _context.BlitzPersonalBests
                        .Where(p => p.Difficulty == "normal" && p.Alphabet == alphabet)
                        .CountAsync(p => p.BestCorrect > myPb.BestCorrect) + 1;

                    selfRow = new
                    {
                        userId = me.Id,
                        name = string.IsNullOrEmpty(me.GamerTag) ? me.UserName : me.GamerTag,
                        bestCorrect = myPb.BestCorrect,
                        bestPoints = myPb.BestPoints,
                        bestCombo = myPb.BestCombo,
                        bestAccuracy = myPb.BestAccuracy,
                        updatedAt = myPb.UpdatedAt,
                        rank = selfRank
                    };
                }
            }

            return Json(new
            {
                alphabet,
                difficulty = "normal",
                rows,
                self = selfRow
            });
        }
    }
}
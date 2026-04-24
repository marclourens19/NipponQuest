using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Models;
using System.Linq;

namespace NipponQuest.Controllers
{
    public class LeaguesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaguesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var allUsers = await _userManager.Users.ToListAsync();

            // Calculate Lists
            var levelList = allUsers.OrderByDescending(u => u.Level).ThenByDescending(u => u.TotalEXP).ToList();
            var streakList = allUsers.OrderByDescending(u => u.LoginStreak).ToList();
            var arenaList = allUsers.Where(u => u.CurrentLeague == currentUser.CurrentLeague).OrderByDescending(u => u.WeeklyXP).ToList();

            int levelRank = levelList.FindIndex(u => u.Id == currentUser.Id) + 1;
            int arenaRank = arenaList.FindIndex(u => u.Id == currentUser.Id) + 1;

            var vm = new LeagueDashboardViewModel
            {
                CurrentUser = currentUser,
                LevelLeaderboard = levelList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),
                StreakLeaderboard = streakList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),
                ArenaRivals = GetRivals(arenaList, arenaRank, currentUser.Id),
                ArenaRank = arenaRank,
                ArenaRankChange = currentUser.LastWeekArenaRank > 0 ? currentUser.LastWeekArenaRank - arenaRank : 0,
                GlobalLevelRank = levelRank,
                GlobalLevelChange = currentUser.LastWeekGlobalRank > 0 ? currentUser.LastWeekGlobalRank - levelRank : 0
            };

            return View(vm); // This must pass 'vm' to fix NullReferenceException
        }

        private List<RankedUserSB> GetRivals(List<ApplicationUser> list, int myRank, string myId)
        {
            int start = Math.Max(0, myRank - 2);
            return list.Skip(start).Take(3).Select((u, i) => new RankedUserSB { Rank = start + i + 1, User = u, IsCurrentUser = u.Id == myId }).ToList();
        }
    }
}
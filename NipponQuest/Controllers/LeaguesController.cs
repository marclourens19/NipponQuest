using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            int totalUsersCount = allUsers.Count;

            var globalOrderedList = allUsers
                .OrderByDescending(u => u.CurrentLeague)
                .ThenByDescending(u => u.Level)
                .ThenByDescending(u => u.TotalEXP)
                .ToList();

            var streakList = allUsers.OrderByDescending(u => u.LoginStreak).ToList();

            var arenaList = allUsers
                .Where(u => u.CurrentLeague == currentUser.CurrentLeague)
                .OrderByDescending(u => u.WeeklyXP)
                .ThenByDescending(u => u.Level)
                .ThenByDescending(u => u.TotalEXP)
                .ToList();

            int globalRank = globalOrderedList.FindIndex(u => u.Id == currentUser.Id) + 1;
            int arenaRank = arenaList.FindIndex(u => u.Id == currentUser.Id) + 1;

            var vm = new LeagueDashboardViewModel
            {
                CurrentUser = currentUser,
                TotalUserCount = totalUsersCount,
                LevelLeaderboard = globalOrderedList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),
                StreakLeaderboard = streakList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),
                ArenaRivals = GetRivals(arenaList, currentUser.Id),
                ArenaRank = arenaRank,
                GlobalLevelRank = globalRank,
                ArenaRankChange = currentUser.LastWeekArenaRank > 0 ? currentUser.LastWeekArenaRank - arenaRank : 0,
                GlobalLevelChange = currentUser.LastWeekGlobalRank > 0 ? currentUser.LastWeekGlobalRank - globalRank : 0
            };

            return View(vm);
        }

        private List<RankedUserSB> GetRivals(List<ApplicationUser> list, string myId)
        {
            return list.Select(u => new RankedUserSB
            {
                Rank = list.IndexOf(u) + 1,
                User = u,
                IsCurrentUser = u.Id == myId
            }).ToList();
        }
    }
}
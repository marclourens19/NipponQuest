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

            // Fetch all users to calculate rankings
            var allUsers = await _userManager.Users.ToListAsync();
            int totalUsersCount = allUsers.Count;

            // 1. GLOBAL RANKING LOGIC: 
            // Priority: League Rank (Enum) -> Level -> Total XP
            var globalOrderedList = allUsers
                .OrderByDescending(u => u.CurrentLeague)
                .ThenByDescending(u => u.Level)
                .ThenByDescending(u => u.TotalEXP)
                .ToList();

            // 2. STREAK RANKING: Global list for sidebar stats
            var streakList = allUsers.OrderByDescending(u => u.LoginStreak).ToList();

            // 3. ARENA RANKING LOGIC: Strictly users in your specific league
            // CRITICAL: Rank based strictly on WeeklyEarnedXP
            // TIE-BREAKER: Added Level and TotalEXP so 0 XP users are ranked fairly (LV.16 beats LV.1)
            var arenaList = allUsers
                .Where(u => u.CurrentLeague == currentUser.CurrentLeague)
                .OrderByDescending(u => u.WeeklyXP)
                .ThenByDescending(u => u.Level)
                .ThenByDescending(u => u.TotalEXP)
                .ToList();

            // Calculate the two distinct rankings
            int globalRank = globalOrderedList.FindIndex(u => u.Id == currentUser.Id) + 1;
            int arenaRank = arenaList.FindIndex(u => u.Id == currentUser.Id) + 1;

            var vm = new LeagueDashboardViewModel
            {
                CurrentUser = currentUser,
                // Passing total user count for the "#X / Total" display
                TotalUserCount = totalUsersCount,

                // Sidebar: Top 3 Global Levels
                LevelLeaderboard = globalOrderedList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),

                // Sidebar: Top 3 Global Streaks
                StreakLeaderboard = streakList.Take(3).Select((u, i) => new RankedUserSB { Rank = i + 1, User = u, IsCurrentUser = u.Id == currentUser.Id }).ToList(),

                // Middle section: Neighbors in the specific Arena (Shows all users in this league)
                ArenaRivals = GetRivals(arenaList, arenaRank, currentUser.Id),

                // Map specific rank values to correct UI slots
                ArenaRank = arenaRank,      // Shows in the Center Card (e.g., #1 if you are highest level at 0xp)
                GlobalLevelRank = globalRank, // Shows in the Global Sidebar (e.g., #5)

                // Track movement if database has LastWeek values
                ArenaRankChange = currentUser.LastWeekArenaRank > 0 ? currentUser.LastWeekArenaRank - arenaRank : 0,
                GlobalLevelChange = currentUser.LastWeekGlobalRank > 0 ? currentUser.LastWeekGlobalRank - globalRank : 0
            };

            return View(vm);
        }

        private List<RankedUserSB> GetRivals(List<ApplicationUser> list, int myRank, string myId)
        {
            // Center the view on the user: Try to show 2 above and 2 below, 
            // but for smaller leagues, we return the full list to ensure #1 is visible.
            int start = Math.Max(0, myRank - 3);

            return list.Skip(start).Take(10).Select(u => new RankedUserSB
            {
                // Pulls the absolute index from the arenaList for accurate numbering (#1, #2, etc)
                Rank = list.IndexOf(u) + 1,
                User = u,
                IsCurrentUser = u.Id == myId
            }).ToList();
        }
    }
}
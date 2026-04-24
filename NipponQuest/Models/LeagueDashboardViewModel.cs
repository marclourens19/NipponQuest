using System.Collections.Generic;

namespace NipponQuest.Models
{
    public class LeagueDashboardViewModel
    {
        public ApplicationUser CurrentUser { get; set; }

        // Sidebar Lists (Matches your sketch)
        public List<RankedUserSB> LevelLeaderboard { get; set; }
        public List<RankedUserSB> StreakLeaderboard { get; set; }

        // Arena Logic
        public List<RankedUserSB> ArenaRivals { get; set; }
        public int ArenaRank { get; set; }
        public int ArenaRankChange { get; set; }

        // Global Stats
        public int GlobalLevelRank { get; set; }
        public int GlobalLevelChange { get; set; }

        // Compatibility Properties to fix View Errors
        public int GlobalRank => GlobalLevelRank;
        public int GlobalRankChange => GlobalLevelChange;
        public List<RankedUserSB> TopGlobalPlayers => LevelLeaderboard;
    }

    public class RankedUserSB
    {
        public int Rank { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
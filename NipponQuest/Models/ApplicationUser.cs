using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace NipponQuest.Models
{
    // Keeping the Enum here inside the namespace fixes the CS0103 error
    public enum LeagueRank { Sprout, Wood, Iron, Gold, Diamond, Emerald, Master, Dragon, Legend }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        public required string GamerTag { get; set; }
        public int Level { get; set; } = 1;
        public int TotalEXP { get; set; } = 0;
        public int CurrentXP { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public int WeeklyXP { get; set; } = 0;
        public int LessonsCompleted { get; set; } = 0;
        public LeagueRank CurrentLeague { get; set; } = LeagueRank.Sprout;

        // --- RANK TRACKING FIELDS ---
        public int LastWeekGlobalRank { get; set; } = 0;
        public int LastWeekArenaRank { get; set; } = 0;

        public int LoginStreak { get; set; } = 0;
        public DateTime? LastLoginDate { get; set; }

        public string LeagueIconClass => CurrentLeague.ToString().ToLower();
        public int RequiredXP => Level <= 10 ? Level * 50 + 50 : Level <= 50 ? Level * 100 : Level * 250;
        public int XPPercentage => RequiredXP <= 0 ? 0 : (int)((double)CurrentXP / RequiredXP * 100);
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NipponQuest.Models
{
    public enum LeagueRank
    {
        Sprout, Wood, Iron, Gold, Diamond, Emerald, Master, Dragon, Legend
    }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(12, MinimumLength = 4, ErrorMessage = "Your Username must be 4-12 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "No special characters allowed!")]
        public required string GamerTag { get; set; }

        public int TotalEXP { get; set; } = 0;
        public int WeeklyXP { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int DailyStreak { get; set; } = 0;
        public int CurrentXP { get; set; } = 0;

        public LeagueRank CurrentLeague { get; set; } = LeagueRank.Sprout;

        // Returns the rank name as a lowercase class (e.g., "legend", "sprout")
        public string LeagueIconClass => CurrentLeague.ToString().ToLower();

        public int RequiredXP
        {
            get
            {
                if (Level <= 10) return Level * 50 + 50;
                if (Level <= 50) return Level * 100;
                return Level * 250;
            }
        }

        public int XPPercentage => (int)((double)CurrentXP / RequiredXP * 100);
    }
}
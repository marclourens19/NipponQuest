using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NipponQuest.Models
{
    /*
     Creating our own ApplicationUser class that inherits from IdentityUser. 
    */
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(12, MinimumLength = 4, ErrorMessage = "Your Username must be 4-12 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "No special characters allowed!")]
        public required string GamerTag { get; set; }

        public int TotalEXP { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int DailyStreak { get; set; } = 0;

        // This is what the progress bar will track (0 to RequiredXP)
        public int CurrentXP { get; set; } = 0;

        // Logic for the Next Level Goal
        public int RequiredXP
        {
            get
            {
                if (Level <= 10) return Level * 50 + 50;  // Lv1=100, Lv2=150...
                if (Level <= 50) return Level * 100;      // Lv11=1100, Lv12=1200...
                return Level * 250;                       // Infinite scaling
            }
        }

        // Percentage for the Bootstrap width style
        public int XPPercentage => (int)((double)CurrentXP / RequiredXP * 100);
    }
} 
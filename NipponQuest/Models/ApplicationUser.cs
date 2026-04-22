// Imported the Identity library so NipponQuest can use Google/Microsoft's built-in security features.
using Microsoft.AspNetCore.Identity;

namespace NipponQuest.Models
{
    /*
     Creating our own ApplicationUser class that inherits from IdentityUser. 
     This allows us to extend the default user properties provided by 
     IdentityUser with our own custom properties in the future if needed.  
    */
    public class ApplicationUser : IdentityUser
    {
        /* 
         Custom properties for the user can be added here. For example, we can track
         the user's total experience points (EXP), gold, level, and daily streak for
         the gamification features of NipponQuest.
        */
        public int TotalEXP { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int DailyStreak { get; set; } = 0;
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Models; // Added to reference ApplicationUser

namespace NipponQuest.Data
{
    // ApplicationDbContext inherits from IdentityDbContext. 
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
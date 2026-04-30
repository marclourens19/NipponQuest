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

        // Blueprint for the Hiragana table in the database.
        // This will allow us to perform CRUD operations on Hiragana records using Entity Framework Core.
        public DbSet<Hiragana> Hiraganas { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
    }
}
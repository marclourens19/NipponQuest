using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;

namespace NipponQuest.Models
{
    public static class SeedData
    {
        // This method is 'static' so we can call it without creating a new instance of SeedData.
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // 'serviceProvider' to get our ApplicationDbContext.
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any Hiragana already in the database
                if (context.Hiraganas.Any())
                {
                    return;   // If there's already data, "Exit Level" and do nothing.
                }

                // If the table is empty, add the "A-Row".
                context.Hiraganas.AddRange(
                    new Hiragana
                    {
                        Symbol = "あ",
                        Romaji = "a",
                        UnlockLevel = 1 // New players can see this immediately.
                    },
                    new Hiragana
                    {
                        Symbol = "い",
                        Romaji = "i",
                        UnlockLevel = 1
                    },
                    new Hiragana
                    {
                        Symbol = "う",
                        Romaji = "u",
                        UnlockLevel = 1
                    },
                    new Hiragana
                    {
                        Symbol = "え",
                        Romaji = "e",
                        UnlockLevel = 1
                    },
                    new Hiragana
                    {
                        Symbol = "お",
                        Romaji = "o",
                        UnlockLevel = 1
                    }
                );

                // This pushes the C# objects into the actual SQL tables.
                context.SaveChanges();
            }
        }
    }
}
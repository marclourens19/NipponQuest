using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;
using NipponQuest.Services;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Security.Claims;

namespace NipponQuest.Controllers
{
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlashcardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void DeepLog(string message)
        {
            string log = $"[DEBUG-DEEP][{DateTime.Now:HH:mm:ss.fff}] {message}";
            Debug.WriteLine(log);
            Console.WriteLine(log);
            Console.Out.Flush();
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var query = _context.Decks
                .Where(d => d.ApplicationUserId == userId)
                .Select(d => new DeckViewModel
                {
                    Id = d.Id,
                    Title = d.Title ?? "Untitled Deck",
                    Description = d.Description ?? "",
                    CardCount = d.Flashcards.Count()
                });

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Title.Contains(searchString) || s.Description.Contains(searchString));

            return View(await query.ToListAsync());
        }

        // GET: Flashcards/Create
        public IActionResult Create() => View();

        // POST: Flashcards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Title,Description")] Deck deck)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                deck.ApplicationUserId = userId;
                ModelState.Remove("ApplicationUserId");

                if (ModelState.IsValid)
                {
                    _context.Decks.Add(deck);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(deck);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ImportAnki(IFormFile ankiFile)
        {
            if (ankiFile == null || ankiFile.Length == 0) return RedirectToAction(nameof(Index));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string workDir = Path.Combine(Directory.GetCurrentDirectory(), "Anki_Handoff", userId);
            string packagePath = Path.Combine(workDir, "upload.apkg");

            try
            {
                if (Directory.Exists(workDir)) Directory.Delete(workDir, true);
                Directory.CreateDirectory(workDir);

                using (var stream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    ankiFile.CopyTo(stream);
                    stream.Flush();
                }

                // Call the cleaned service
                var cardBuffer = AnkiProcessor.GetCardsFromPackage(packagePath, workDir);

                ViewBag.DeckName = Path.GetFileNameWithoutExtension(ankiFile.FileName);
                return View("~/Views/Flashcards/ConfirmImport.cshtml", cardBuffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRASH]: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult FinalizeImport(string deckTitle, List<Flashcard> cards)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || cards == null || !cards.Any()) return RedirectToAction(nameof(Index));

            var newDeck = new Deck { Title = deckTitle, Description = "Imported from Anki", ApplicationUserId = userId };
            _context.Decks.Add(newDeck);
            _context.SaveChanges();

            foreach (var card in cards) { card.DeckId = newDeck.Id; card.Deck = null; }
            _context.Flashcards.AddRange(cards);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == id && d.ApplicationUserId == userId);

            if (deck != null) _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
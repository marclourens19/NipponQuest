using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NipponQuest.Data;
using NipponQuest.Models;
using NipponQuest.Services;
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
            {
                query = query.Where(s =>
                    s.Title.Contains(searchString) ||
                    s.Description.Contains(searchString));
            }

            return View(await query.ToListAsync());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ImportAnki(IFormFile ankiFile)
        {
            DeepLog(">>> ImportAnki Started.");

            if (ankiFile == null || ankiFile.Length == 0)
            {
                DeepLog("ERROR: No file uploaded.");
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            string baseTemp = Path.Combine(Path.GetTempPath(), "Anki_Handoff");
            string workDir = Path.Combine(baseTemp, Guid.NewGuid().ToString("N"));

            string packagePath = Path.Combine(workDir, "upload.apkg");

            try
            {
                Directory.CreateDirectory(workDir);

                DeepLog("Saving uploaded file...");

                await using (var fs = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                await using (var inputStream = ankiFile.OpenReadStream())
                {
                    await inputStream.CopyToAsync(fs);
                }

                DeepLog("STAGE 1 COMPLETE - FILE SAVED");

                if (!System.IO.File.Exists(packagePath))
                    throw new Exception("File not saved correctly.");

                var fileSize = new FileInfo(packagePath).Length;
                DeepLog($"File size: {fileSize}");

                if (fileSize == 0)
                    throw new Exception("Uploaded file is empty.");

                DeepLog("STAGE 2: Processing Anki file...");

                var cards = AnkiProcessor.GetCardsFromPackage(packagePath, workDir);

                if (cards == null || cards.Count == 0)
                {
                    DeepLog("No cards extracted.");
                    return RedirectToAction(nameof(Index));
                }

                DeepLog($"STAGE 2 SUCCESS: {cards.Count} cards");

                ViewBag.DeckName = Path.GetFileNameWithoutExtension(ankiFile.FileName);
                return View("~/Views/Flashcards/ConfirmImport.cshtml", cards);
            }
            catch (Exception ex)
            {
                DeepLog("CRITICAL IMPORT ERROR: " + ex);
                return RedirectToAction(nameof(Index));
            }
            finally
            {
                try
                {
                    if (Directory.Exists(workDir))
                        Directory.Delete(workDir, true);
                }
                catch (Exception ex)
                {
                    DeepLog("Cleanup error: " + ex.Message);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeImport(string deckTitle, List<Flashcard> cards)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || cards == null || !cards.Any())
                return RedirectToAction(nameof(Index));

            var newDeck = new Deck
            {
                Title = deckTitle ?? "Imported Deck",
                Description = "Imported from Anki",
                ApplicationUserId = userId
            };

            _context.Decks.Add(newDeck);
            await _context.SaveChangesAsync();

            foreach (var card in cards)
            {
                card.DeckId = newDeck.Id;
                card.Deck = null;
            }

            _context.Flashcards.AddRange(cards);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var deck = await _context.Decks
                .FirstOrDefaultAsync(d => d.Id == id && d.ApplicationUserId == userId);

            if (deck != null)
                _context.Decks.Remove(deck);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Deck deck)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                deck.ApplicationUserId = userId;
                ModelState.Remove("ApplicationUserId");

                if (ModelState.IsValid)
                {
                    _context.Add(deck);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(deck);
        }
    }
}
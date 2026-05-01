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

        public IActionResult Create() => View();

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

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == id && d.ApplicationUserId == userId);
            if (deck == null) return NotFound();
            return View(deck);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Deck deck)
        {
            if (id != deck.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    deck.ApplicationUserId = userId!;
                    _context.Update(deck);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Decks.Any(e => e.Id == deck.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(deck);
        }

        [HttpGet]
        public IActionResult AddCard(int deckId)
        {
            return View(new Flashcard { DeckId = deckId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard([Bind("DeckId,FrontText,BackText,ImageFilePath,AudioFilePath")] Flashcard flashcard)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flashcard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flashcard);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deck = await _context.Decks
                .Include(d => d.Flashcards)
                .FirstOrDefaultAsync(d => d.Id == id && d.ApplicationUserId == userId);

            if (deck != null) _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Study(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deck = await _context.Decks
                .Include(d => d.Flashcards)
                .FirstOrDefaultAsync(d => d.Id == id && d.ApplicationUserId == userId);

            if (deck == null) return NotFound();

            var cardToStudy = deck.Flashcards
                .OrderBy(f => f.SuccessCount)
                .ThenBy(f => Guid.NewGuid())
                .FirstOrDefault();

            if (cardToStudy == null) return RedirectToAction("Index");

            ViewBag.DeckTitle = deck.Title;
            return View(cardToStudy);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResult(int cardId, string result)
        {
            var card = await _context.Flashcards.FindAsync(cardId);
            if (card == null) return NotFound();

            // ANKI LOGIC ENGINE
            switch (result.ToLower())
            {
                case "again": card.SuccessCount = 0; break;
                case "hard": card.SuccessCount = Math.Max(0, card.SuccessCount - 1); break;
                case "good": card.SuccessCount += 1; break;
                case "easy": card.SuccessCount += 3; break;
            }

            card.LastReviewed = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction("Study", new { id = card.DeckId });
        }
    }
}
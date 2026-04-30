using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NipponQuest.Models
{
    public class Deck
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
    }

    public class Flashcard
    {
        public int Id { get; set; }

        [Required]
        public int DeckId { get; set; }

        [ForeignKey("DeckId")]
        public Deck? Deck { get; set; }

        public string FrontText { get; set; } = string.Empty;
        public string BackText { get; set; } = string.Empty;

        public string ImageFilePath { get; set; } = string.Empty;
        public string AudioFilePath { get; set; } = string.Empty;
    }

    public class DeckViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CardCount { get; set; }
    }
}
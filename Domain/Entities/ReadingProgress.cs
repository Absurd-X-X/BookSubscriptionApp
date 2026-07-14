namespace Domain.Entities
{
    public class ReadingProgress
    {
        public Guid Id { get; set; }

        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;

        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;

        // Current Progress
        public int CurrentPage { get; set; }
        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }

        // EPUB
        public string? Cfi { get; set; }
        public string? CurrentChapter { get; set; }

        // Statistics
        public int TotalMinutesRead { get; set; }
        public int TotalPagesRead { get; set; }

        // Reading Streak
        public DateTime? LastReadDate { get; set; }
        public int ReadingStreak { get; set; }
        public int LongestReadingStreak { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
    }
}

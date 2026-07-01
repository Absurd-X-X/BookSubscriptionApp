namespace Domain.Entities
{
    public class ReadingProgress
    {
        public Guid Id { get; set; }

        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;

        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;

        public double Percentage { get; set; }

        public string? CurrentLocation { get; set; }

        public DateTime LastReadAt { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set;}
    }
}

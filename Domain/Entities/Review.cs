namespace Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;
        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;
        public int Rating { get; set; }
        public string Comment { get; set; } = default!;
        public bool IsApproved { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime? EditedAt { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}

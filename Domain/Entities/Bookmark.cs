namespace Domain.Entities
{
    public class Bookmark
    {
        public Guid Id { get; set; }

        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;

        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;

        public int PageNumber { get; set; }
        public string Quote { get; set; } = default!;
        public string Note { get; set; } = default!;

        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
    }
}
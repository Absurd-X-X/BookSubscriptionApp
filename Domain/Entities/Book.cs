namespace Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string Isbn { get; set; } = default!;
        public int Pages { get; set; }
        public int PublicationYear { get; set; }
        public string Genre { get; set; } = default!;
        public Guid LibraryId { get; set; }
        public Library Library { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public string BookFileUrl { get; set; } = default!;
        public string FileType { get; set; } = default!;
        public string BookCoverUrl { get; set; } = default!;
        public int NoOfTimeReadByPeople { get; set; }
        public string About { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPublished { get; set; }
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new HashSet<ReadingProgress>();
    }
}

namespace Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Basic Details (Step 1)
        public string Title { get; set; } = default!;
        public string? Subtitle { get; set; }
        public string Author { get; set; } = default!;
        public string Publisher { get; set; } = default!;
        public int PublicationYear { get; set; }
        public string Language { get; set; } = default!;
        public string Isbn { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public string Genre { get; set; } = default!;
        public string About { get; set; } = default!;
        public int Pages { get; set; }
        public string MimeType { get; set; } = default!;

        // Pricing & Access (Step 2)
        public string PricingType { get; set; } = "Free"; // Free | Paid | Subscription
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string AccessLevel { get; set; } = "Everyone"; // Everyone | SubscribersOnly | PremiumMembers
        public bool RequireLogin { get; set; } = true;
        public bool AllowDownload { get; set; }
        public bool AllowPrint { get; set; }
        public bool AllowCopyPaste { get; set; }

        // Files (Step 3 — current active file; version history lives in BookVersion)
        public string BookFileUrl { get; set; } = default!;
        public string FileType { get; set; } = default!;
        public string BookCoverUrl { get; set; } = default!;

        // System
        public Guid LibraryId { get; set; }
        public Library Library { get; set; } = default!;
        public int NoOfTimeReadByPeople { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPublished { get; set; }
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<Favorite> Favorites{ get; set; } = new HashSet<Favorite>();
        public ICollection<ReadingListItem> ReadingListItems { get; set; } = new HashSet<ReadingListItem>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new HashSet<Bookmark>();
        public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new HashSet<ReadingProgress>();
        public ICollection<BookVersion> Versions { get; set; } = new HashSet<BookVersion>();
    }
}
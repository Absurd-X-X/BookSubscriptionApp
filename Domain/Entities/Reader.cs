using Domain.Enums;

namespace Domain.Entities
{
    public class Reader
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public ReadingGoalType? ReadingGoalType { get; set; }   // null = no goal set yet
        public int? ReadingGoalTarget { get; set; }
        public DateTime? ReadingGoalDeadline { get; set; }
        public string? ReadingGoalMotivation { get; set; }
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();
        public ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
        public ICollection<ReadingListItem> Readings { get; set; } = new HashSet<ReadingListItem>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new HashSet<Bookmark>();
        public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new HashSet<ReadingProgress>();
    }
}

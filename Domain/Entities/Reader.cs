namespace Domain.Entities
{
    public class Reader
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new HashSet<ReadingProgress>();
    }
}

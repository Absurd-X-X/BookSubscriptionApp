namespace Domain.Entities
{
    public class Library
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string RefNumber { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public ICollection<Book> Books { get; set; } = new HashSet<Book>();
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}

namespace Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public DateTime? LastMessageAt { get; set; }
        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
        public ICollection<UserConversation> UserConversations { get; set; } = new HashSet<UserConversation>();
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}

namespace Domain.Entities
{
    public class UserConversation
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid ConversationId { get; set; }
        public bool IsAdmin { get; set; }
        public Conversation Conversation { get; set; } = default!;
    }
}
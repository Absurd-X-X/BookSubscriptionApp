namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string HashPassword { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public Wallet? Wallet { get; set; }
        public Reader? Reader { get; set; }
        public Library? Library { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiry { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<BankAccount> BankAccounts { get; set; } = new HashSet<BankAccount>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
        public ICollection<Notification> Notifications { get; set;} = new HashSet<Notification>();
        public ICollection<UserConversation> UserConversations { get; set; } = new HashSet<UserConversation>();
    }
}

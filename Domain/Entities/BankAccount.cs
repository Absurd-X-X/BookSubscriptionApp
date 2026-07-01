namespace Domain.Entities
{
    public class BankAccount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AccountName { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string RecipientCode { get; set; } = default!;
        public string BankName { get; set; } = default!;
        public string BankCode { get; set; } = default!;
        public bool IsDefault { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}


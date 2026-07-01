using Domain.Enums;

namespace Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;
        public decimal Balance { get; set; }
        public TransactionType Type { get; set; }
        public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;
        public string? PaystackReference { get; set; }
        public string Description { get; set; } = default!;
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}

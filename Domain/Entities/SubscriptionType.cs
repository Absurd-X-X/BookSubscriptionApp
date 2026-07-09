using Domain.Enums;

namespace Domain.Entities
{
    public class SubscriptionType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeName = default!;
        public BillingCycle Cycle { get; set; }
        public decimal Cost { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();
        public DateTime ExpiryDate => Cycle switch
        {
            BillingCycle.Monthly => SubscriptionDate.AddMonths(1),
            BillingCycle.Quaterly => SubscriptionDate.AddMonths(4),
            BillingCycle.SemiAnnually => SubscriptionDate.AddMonths(6),
            BillingCycle.Yearly => SubscriptionDate.AddYears(1),

            _ => throw new ArgumentOutOfRangeException(nameof(Cycle), $"Unknown billing cycle: {Cycle} detected!!!")
        };
    }
}

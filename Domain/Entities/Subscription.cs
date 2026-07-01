namespace Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;
        public bool AutoRenewal { get; set; }
        public Guid SubscriptionTypeId { get; set; }
        public SubscriptionType Types { get; set; } = default!;
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }

    }
}

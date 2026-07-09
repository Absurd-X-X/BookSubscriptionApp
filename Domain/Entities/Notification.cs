using Domain.Enums;

namespace Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string Ref { get; set; } = default!;
        public NotificationRefType RefType { get; set; } = NotificationRefType.None;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public NotificationType Type { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}

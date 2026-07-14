using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public User User { get; set; } = default!;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Icon { get; set; } = default!;

        public string UserRole { get; set; } = default!;

        public string ActionType { get; set; } = default!; // e.g., "Create", "Update", "Delete"

        public ResourceType ResourceType { get; set; }

        public Guid? ResourceId { get; set; }

        public string Description { get; set; } = default!; // Detailed log message

        public string IpAddress { get; set; } = default!; // Optional: IP address of the user performing the action
        public bool IsDeleted { get; set; } = false; // Soft delete flag
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey ("UserId")]
        public User User { get; set; } = default!; // Navigation property to the User entity

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string Icon { get; set; } = default!;

        [Required]
        [StringLength(150)]
        public string UserRole { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = default!; // e.g., "Create", "Update", "Delete"

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = default!; // Detailed log message

        [StringLength(50)]
        public string IpAddress { get; set; } = default!; // Optional: IP address of the user performing the action
        public bool IsDeleted { get; set; } = false; // Soft delete flag
    }
}

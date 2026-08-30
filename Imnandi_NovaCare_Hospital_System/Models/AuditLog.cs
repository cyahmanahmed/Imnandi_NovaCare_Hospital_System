using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }
        [Required]
        public string ActionTaken { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? Entity { get; set; }
        public string? RecordId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public string? SessionId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? LoginDateTime { get; set; } 
        public DateTime? LogoutDateTime { get; set; } 
    }
}

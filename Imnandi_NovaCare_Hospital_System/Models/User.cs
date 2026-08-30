using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName name is required")]
        [StringLength(100, ErrorMessage = "LastName name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty; 

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;
        public virtual Employee Employee { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}

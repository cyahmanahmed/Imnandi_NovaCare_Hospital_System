using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } 

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public bool IsRead { get; set; } = false;

        public virtual User User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public string TargetRole { get; set; }
        public virtual ICollection<AlertRead> AlertReads { get; set; }
            = new List<AlertRead>();
    }
}

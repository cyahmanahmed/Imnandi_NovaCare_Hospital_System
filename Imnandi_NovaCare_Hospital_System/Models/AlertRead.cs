using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AlertRead
    {
        [Key]
        public int AlertReadId { get; set; }

        [Required]
        public int AlertId { get; set; }

        [ForeignKey(nameof(AlertId))]
        public virtual Alert Alert { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }


        public bool IsRead { get; set; } = true;

        public DateTime ReadAt { get; set; } = DateTime.Now;
    }
}

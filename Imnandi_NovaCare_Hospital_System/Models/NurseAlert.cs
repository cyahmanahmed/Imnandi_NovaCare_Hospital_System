using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class NurseAlert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NurseId { get; set; }
        public Nurse Nurse { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class NursePerformanceNote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NurseId { get; set; }
        public Nurse Nurse { get; set; }

        [Required]
        [StringLength(1000)]
        public string Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

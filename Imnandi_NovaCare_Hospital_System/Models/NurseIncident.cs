using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class NurseIncident
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NurseId { get; set; }
        public Nurse Nurse { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime IncidentDate { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedDate { get; set; }
    }
}
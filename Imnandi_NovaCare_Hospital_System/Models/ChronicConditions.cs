using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ChronicConditions
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }  

        public string Description { get; set; }  

        public DateOnly? DiagnosedDate { get; set; }

        public bool IsActive { get; set; } = true; 


        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}

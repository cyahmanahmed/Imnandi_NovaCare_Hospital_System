using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class MedicalHistory
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public int? PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
    }
}

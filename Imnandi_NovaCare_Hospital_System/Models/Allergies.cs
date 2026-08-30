using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Allergies
    {

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Allergy name is required")]
        [StringLength(100)]
        public string Allergen { get; set; } = null!;
        public int? PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Medication
    {
        [Key]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Medication name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string MedicationName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
        public int? QuantityOnHand { get; set; }

        [StringLength(50, ErrorMessage = "Dosage form (e.g. tablet, liquId) cannot exceed 50 characters")]
        public string DosageForm { get; set; }

        [StringLength(100, ErrorMessage = "Manufacturer name cannot exceed 100 characters")]
        public string Manufacturer { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Prescription> Prescription { get; set; } = new List<Prescription>();
        public virtual ICollection<MedicationAdministration> MedicationAdministration { get; set; } = new List<MedicationAdministration>();
    }
}

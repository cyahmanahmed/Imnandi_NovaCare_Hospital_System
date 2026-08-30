using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class MedicationAdministration
    {
        [Key]
        public int MedicationAdministrationId { get; set; }



        public string Purpose { get; set; }

        [Required(ErrorMessage = "Administration time is required")]
        public DateOnly AdministrationTime { get; set; }

        [Required(ErrorMessage = "Dosage is required")]
        [StringLength(500, ErrorMessage = "Dosage cannot exceed 500 characters")]
        public string Dosage { get; set; }

        [Required(ErrorMessage = "Medication Id is required")]
        public int MedicationId { get; set; }
        public bool IsSeen { get; set; } = false;


        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        [ForeignKey("MedicationId")]
        public virtual Medication Medication { get; set; }
        public int? NurseId { get; set; }
        public Nurse Nurse { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public int? NurseSisterId { get; set; }
        public NurseSister NurseSister { get; set; }

        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }
    }
}

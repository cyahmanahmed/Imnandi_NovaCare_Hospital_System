using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Prescription
    { 
        [Key]
        public int PrescriptionId { get; set; }
        public int MedicationId { get; set; }

        public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public TimeOnly IssueTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

        public int? ScriptManagerId {  get; set; }
        public ScriptManager ScriptManager { get; set; }

        public int PatientId {  get; set; }
        public Patient Patient { get; set; }

        public int DoctorId {  get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("MedicationId")]
        public virtual Medication Medication { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string Status { get; set; } = "Pending";
        public string DosageInstructions { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
        public int? Level { get; set; }
    }
}

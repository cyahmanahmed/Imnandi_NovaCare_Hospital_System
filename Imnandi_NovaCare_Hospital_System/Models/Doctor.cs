using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.SymbolStore;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId {  get; set; }
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public bool IsAvail { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        [Required]
        public int EmployeeId { get; set; }



        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
        public int? WardId { get; set; }

        [ForeignKey("WardId")]
        public virtual Ward Ward { get; set; }
        public Collection<Patient>Patients { get; set; }
        public Collection<DischargeInstruction> DischargeInstructions { get; set; }
        public Collection<MedicationAdministration> MedicationAdministration { get; set; }
        public Collection<Instruction>Instructions { get; set; }
        public Collection<Visit>Vists { get; set; }
        public Collection<Treatment> Treatments { get; set; }
        public Collection<Prescription>Prescriptions { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<PatientHistory> PatientHistories { get; set; }
    }
}

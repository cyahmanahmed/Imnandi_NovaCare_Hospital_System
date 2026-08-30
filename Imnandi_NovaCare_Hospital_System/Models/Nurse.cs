using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Nurse
    {
        [Key]
        public int NurseId {  get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Department {  get; set; }
        public bool IsAvail { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;


        public int? WardId {  get; set; }
        public Ward ward { get; set; }
        public string WardName { get; set; }

        public Collection<Patient>Patients { get; set; }
        public Collection<VitalSign>VitalSigns { get; set; }
        public Collection<MedicationAdministration> MedicationAdministration { get; set; }
        public ICollection<NursePerformanceNote> PerformanceNotes { get; set; } = new List<NursePerformanceNote>();
        public ICollection<NurseIncident> Incidents { get; set; } = new List<NurseIncident>();
        public ICollection<NurseAlert> Alerts { get; set; } = new List<NurseAlert>();
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardAdmin
    {
        [Key]
        public int WardAdminId {  get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Department { get; set; }
        public bool IsAvail { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        public int? WardId {  get; set; }
        public Ward Ward { get; set; }

        public Collection<Patient> Patient { get; set; }
        public Collection<AdmissionFolder>AdmissionFolders { get; set; }

        public Collection<PatientMovement> PatientMovement { get; set; }
        public Collection<Discharge>Discharges { get; set; }

    }
}

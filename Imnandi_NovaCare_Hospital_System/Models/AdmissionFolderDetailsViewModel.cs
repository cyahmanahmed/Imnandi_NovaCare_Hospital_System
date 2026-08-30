using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AdmissionFolderDetailsViewModel
    {
        [Required]
        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [Display(Name = "Reason for Admission")]
        public string ReasonForAdmission { get; set; } = string.Empty;

        [Display(Name = "Ward")]
        public string WardName { get; set; } = "N/A";

        [Display(Name = "Room")]
        public string RoomNumber { get; set; } = "N/A";

        [Display(Name = "Bed")]
        public string BedNumber { get; set; } = "N/A";

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Date Closed")]
        public DateTime? DateClosed { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string DoctorName { get; set; }
        public string NurseName { get; set; }
        public List<Instruction> Instructions { get; set; }
        public List<PatientMovement> PatientMovements { get; set; }
        public List<VitalSign> VitalSigns { get; set; }
        public List<Prescription> Prescriptions { get; set; }
        public List<DischargeInstruction> DischargeInstructions { get; set; }
        public List<Treatment> Treatments { get; set; }
        public bool HasPendingDischarge { get; set; } = false;
        public bool HasPendingMove { get; set; }=false;
        public List<PatientMoveRequest> PendingMoveRequests { get; set; } = new List<PatientMoveRequest>();
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name must be 50 characters or less")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name must be 50 characters or less")]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(13)]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10, ErrorMessage = "Gender must be 10 characters or less")]
        public string Gender { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Current Medication cannot exceed 500 characters")]
        public string CurrentMedication { get; set; }

        [Required]
        public DateTime DateAdmitted { get; set; } = DateTime.UtcNow;

        public DateTime? DateDischarged { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }

        [Required]
        public bool IsDischarged { get; set; } = false;

        [Required(ErrorMessage = "Bank Name is required")]
        [StringLength(50)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Bank Account Number is required")]
        [StringLength(50)]
        public string BankAccountNumber { get; set; }

        public bool IsInsured { get; set; } = false;
        [StringLength(100)]
        public string InsuranceProvider { get; set; }

        [StringLength(50)]
        public string PolicyNumber { get; set; }

        [StringLength(100)]
        public string MedicalAidPlan { get; set; }

        [StringLength(100)]
        public string MainMemberName { get; set; }
        public int AdmissionCount { get; set; } = 0;
        public string AdmissionHistory { get; set; } 
        public DateTime? LastDischargeDate { get; set; } 

        public int? WardAdminId { get; set; }
        public WardAdmin WardAdmin { get; set; }

        public int? DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int? NurseId { get; set; }
        public Nurse Nurse { get; set; }

        public int? RoomId { get; set; }
        public Room Room { get; set; }

        public int? BedId { get; set; }
        public Bed Bed { get; set; }

        public bool IsDeleted { get; set; } = false;

        public int? AdmissionFolderId { get; set; }
        public AdmissionFolder AdmissionFolder { get; set; }


        public ICollection<PatientMovement> PatientMovements { get; set; } = new List<PatientMovement>();
        public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Instruction> Instructions { get; set; } = new List<Instruction>();
        public ICollection<Visit> Vists { get; set; } = new List<Visit>();
        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
        public ICollection<MedicalHistory> MedicalHistory { get; set; } = new List<MedicalHistory>();
        public ICollection<MedicationAdministration> MedicationAdministration { get; set; } = new List<MedicationAdministration>();
        public ICollection<Allergies> Allergies { get; set; } = new List<Allergies>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<ChronicConditions> ChronicConditions { get; set; } = new List<ChronicConditions>();
        public ICollection<DischargeInstruction> DischargeInstructions { get; set; } = new List<DischargeInstruction>();
        public ICollection<PatientMoveRequest> PatientMoveRequest { get; set; } = new List<PatientMoveRequest>();
        public ICollection<Discharge> Discharges { get; set; }
        public ICollection<PatientHistory> PatientHistories { get; set; }
    }
}

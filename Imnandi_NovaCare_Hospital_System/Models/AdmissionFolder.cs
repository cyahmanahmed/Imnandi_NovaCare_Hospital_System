using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AdmissionFolder
    {
        [Key]
        public int AdmissionFolderId { get; set; }
        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public int? BedId { get; set; }
        public Bed Bed { get; set; }
        [Required]
        public int? WardAdminId { get; set; }
        public WardAdmin WardAdmin { get; set; }
        [Required]
        [StringLength(300)]
        public string ReasonForAdmission { get; set; }
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateClosed { get; set; }
        [Required]
        public bool IsActive { get; set; } = false;
        public bool HasPendingDischarge { get; set; } = false;
        public int AdmissionCount { get; set; } = 0;



        public ICollection<PatientMovement> PatientMovements { get; set; } = new List<PatientMovement>();
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Instruction> Instructions { get; set; } = new List<Instruction>();
    }
}
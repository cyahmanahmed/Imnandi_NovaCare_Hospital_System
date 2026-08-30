using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class PatientMoveRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public int TargetWardId { get; set; } 
        public Ward TargetWard { get; set; }

        [Required]
        public string Reason { get; set; } 

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public int? WardAdminId { get; set; } 
        public WardAdmin WardAdmin { get; set; }

        public DateTime? ProcessedDate { get; set; }
        public string Status { get; set; } = "Pending"; 

        public string Notes { get; set; } 
    }

}

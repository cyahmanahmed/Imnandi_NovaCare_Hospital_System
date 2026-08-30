using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class DischargeInstruction
    {
        [Key]
        public int Id { get; set; }


        public  int DoctorId {  get; set; }
        public Doctor Doctor { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.Now;
        [Required]
        [StringLength(200)]
        public string Reason { get; set; }
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; 
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public Admin ApprovedByAdmin { get; set; }
    }
}

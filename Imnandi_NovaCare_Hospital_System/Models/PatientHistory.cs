using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class PatientHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        public int? DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }
        [Required]
        public DateTime AdmissionDate { get; set; }

        public DateTime? DischargeDate { get; set; }
        [StringLength(200)]
        public string TreatmentType { get; set; }

        [StringLength(1000)]
        public string TreatmentDescription { get; set; }
        public int? WardId { get; set; }
        [ForeignKey(nameof(WardId))]
        public Ward Ward { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; }

        public int? BedId { get; set; }
        [ForeignKey(nameof(BedId))]
        public Bed Bed { get; set; }
    }
}

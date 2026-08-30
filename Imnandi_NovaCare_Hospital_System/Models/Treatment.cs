using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Treatment
    {
        [Key]
        public int TreatmentID { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string PatientName { get; set; }
        public DateOnly TreatmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string TreatmentType { get; set; }
        public string Description { get; set; }
        public string PastTreatment { get; set; }
    }
}

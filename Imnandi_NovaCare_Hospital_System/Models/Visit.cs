using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Visit
    {
        [Key]
        public int VisitId { get; set; }
        public int DoctorId { get; set;}
        public Doctor Doctor { get; set;}
        public int PatientId {  get; set;}
        public Patient Patient { get; set;}
        public DateTime VisitDateTime { get; set; }
        public string Purpose { get; set; } = null!;
    }
}

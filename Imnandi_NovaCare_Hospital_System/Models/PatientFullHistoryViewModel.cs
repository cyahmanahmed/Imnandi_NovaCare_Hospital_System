namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class PatientFullHistoryViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string IdNumber { get; set; }
        public DateTime? DateAdmitted { get; set; }
        public DateTime? DateDischarged { get; set; }
        public string CurrentRoom { get; set; }
        public string CurrentBed { get; set; }
        public List<PatientMovement> Movements { get; set; } = new List<PatientMovement>();
        public List<Treatment> Treatments { get; set; } = new List<Treatment>();
    }
}

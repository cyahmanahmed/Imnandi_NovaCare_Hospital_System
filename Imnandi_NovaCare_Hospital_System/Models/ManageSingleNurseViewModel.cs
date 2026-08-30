namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ManageSingleNurseViewModel
    {
        public Nurse Nurse { get; set; }

        public List<NursePerformanceNote> PerformanceNotes { get; set; }
            = new();

        public List<NurseIncident> Incidents { get; set; }
            = new();

        public List<NurseAlert> Alerts { get; set; }
            = new();
    }
}

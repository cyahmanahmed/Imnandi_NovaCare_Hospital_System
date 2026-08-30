using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Discharge
    {
        [Key]
        public int DischargeId {  get; set; }


        public int WardAdminId {  get; set; }
        public WardAdmin WardAdmin { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public DateTime DischargeDate { get; set; }
        public string TreatmentSummary { get; set; }
        public string FolloupInstructions { get; set; }
    }
}

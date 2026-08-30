using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class VitalSign
    {

        [Key]
        public int VitalSignId { get; set; }
        public double BodyTemperature { get; set; }
        public string PulseRate { get; set; }
        public string RespiratoryRate { get; set; }
        public string BloodPressure { get; set; }
        public string OxygenSaturation { get; set; }

        [Required]
        public int? PatientId { get; set; }
        public Patient Patient { get; set; }
        [Required]
        public int NurseId { get; set; }
        public Nurse Nurse { get; set; }
        [Required]
        public int HeartRate { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }
        [Required]
        public int DoctorId { get; set; }      
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }    
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public string Location { get; set; }    

        public string VisitType { get; set; } 

        public bool IsCompleted { get; set; } = false; 
        public bool IsDeleted { get; set; }=false; 
    }
}

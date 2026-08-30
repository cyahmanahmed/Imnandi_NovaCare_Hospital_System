using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class PatientMovement
    {
        [Key]
        public int PatientMovementID { get; set; }
        public int PatientId {  get; set; } 
        public Patient Patient { get; set; }

        public int WardAdminId {  get; set; }
        public WardAdmin WardAdmin { get; set; }
        [Required]
        public string MovementType { get; set; }
        public int? FromBedId { get; set; }
        public Bed FromBed { get; set; }

        public int? FromRoomId { get; set; }
        public Room FromRoom { get; set; }
        public int? ToBedId { get; set; }
        public Bed ToBed { get; set; }

        public int? ToRoomId { get; set; }
        public Room ToRoom { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Now; 
        public string MovementHistory { get; set; }
        public DateTime? ReturnTime { get; set; }
        public bool IsTemporaryMove => MovementType == "Temporary Move";
        public int? AdmissionFolderId { get; set; }
        public AdmissionFolder AdmissionFolder { get; set; }
    }
}

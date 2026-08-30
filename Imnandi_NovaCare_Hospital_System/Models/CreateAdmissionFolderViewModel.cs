using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class CreateAdmissionFolderViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        [Required]
        public int? RoomId { get; set; }
        [Required]
        public int? BedId { get; set; }


        [Required]
        public string ReasonForAdmission { get; set; }
        public List<Ward> Wards { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();
        public List<Bed> Beds { get; set; } = new();
        [Required]
        public int? DoctorId { get; set; }
        [Required]
        public int? NurseId { get; set; }
        public List<Doctor> Doctors { get; set; } = new();
        public List<Nurse> Nurses { get; set; } = new();
        public Doctor AssignedDoctor { get; set; }
        public Nurse AssignedNurse { get; set; }
        public Bed AssignedBed { get; set; }
        public Room AssignedRoom { get; set; }
        public string VisitType { get; set; }
    }
}

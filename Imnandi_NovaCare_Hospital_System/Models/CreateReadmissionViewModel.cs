using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class CreateReadmissionViewModel
    {
        public int PatientId { get; set; }

        public string PatientName { get; set; } = null!;

        [Required(ErrorMessage = "Room selection is required")]
        public int? RoomId { get; set; }

        [Required(ErrorMessage = "Bed selection is required")]
        public int? BedId { get; set; }

        [Required(ErrorMessage = "Reason for readmission is required")]
        public string ReasonForReadmission { get; set; } = null!;


        public int? PreviousAdmissionFolderId { get; set; }
        public AdmissionFolder PreviousAdmissionFolder { get; set; }


        public List<Ward> Wards { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();
        public List<Bed> Beds { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<Nurse> Nurses { get; set; } = new();


        [Required(ErrorMessage = "Doctor selection is required")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Nurse selection is required")]
        public int? NurseId { get; set; }

        public Doctor AssignedDoctor { get; set; }
        public Nurse AssignedNurse { get; set; }
        public Bed AssignedBed { get; set; }
        public Room AssignedRoom { get; set; }
        public string VisitType { get; set; }
        public bool IsReadmission { get; set; } = true;
        public DateTime? LastDischargeDate { get; set; }
    }

}

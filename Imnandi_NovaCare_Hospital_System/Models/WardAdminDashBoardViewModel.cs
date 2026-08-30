using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{

    public class WardAdminDashBoardViewModel
    {
        [Required(ErrorMessage = "Nurse name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Nurse surname is required")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalPatients { get; set; }
        public int PatientsInHospital { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalNurseSisters { get; set; }
        public int TotalNurses { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalWards { get; set; }
        public int BedsAvailable { get; set; }
        public List<string> RecentRegistrations { get; set; } = new List<string>();
        public List<Patient> Patient { get; set; } = new();
        public string PhoneNumber { get; set; }
        public List<DischargeInstruction> DischargeInstructions { get; set; } = new List<DischargeInstruction>();
    }
}

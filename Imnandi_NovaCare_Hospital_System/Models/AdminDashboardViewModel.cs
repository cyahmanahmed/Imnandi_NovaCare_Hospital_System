using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AdminDashboardViewModel
    {

        [Required(ErrorMessage = "Administrator name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Administrator surname is required")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [StringLength(100)]
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }


        public int TotalPatients { get; set; }
        public int PatientsInHospital { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalNurseSisters { get; set; }
        public int TotalNurses { get; set; }
        public int TotalRooms { get; set; }
        public int TotalScriptManagers { get; set; }
        public int TotalStockManagers { get; set; }
        public int TotalWardAdmins { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalWards { get; set; }
        public int BedsAvailable { get; set; }


        public List<string> RecentRegistrations { get; set; } = new List<string>();
        public int RecentRegistrationsCount => RecentRegistrations?.Count ?? 0;


        public List<Alert> RecentAlerts { get; set; } = new List<Alert>();
        public int AlertCount => RecentAlerts?.Count ?? 0;
        public List<dynamic> RecentAdmissions { get; set; } = new List<dynamic>();
        public List<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();
        public int? AuditLogCount => RecentAuditLogs?.Count ?? 0;

    }

}

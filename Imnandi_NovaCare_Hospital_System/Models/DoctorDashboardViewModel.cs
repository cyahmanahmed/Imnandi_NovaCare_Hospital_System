using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public int EmployeeId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string UserName { get; internal set; }

        public string JobTitle { get; set; }
        public string Specialty { get; set; }
        public string Department { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }


        public int PatientCount { get; set; }
        public int UpcomingVisitCount => UpcomingVisits?.Count ?? 0;
        public int RecentPrescriptionCount => RecentPrescriptions?.Count ?? 0;
        public int PendingDischargeCount => PendingDischarges?.Count ?? 0;


        public List<Patient> Patients { get; set; } = new();
        public List<Visit> UpcomingVisits { get; set; } = new();
        public List<Schedule> UpcomingSchedules { get; set; } = new();
        public List<Prescription> RecentPrescriptions { get; set; } = new();
        public List<DischargeInstruction> PendingDischarges { get; set; } = new();
    }
}

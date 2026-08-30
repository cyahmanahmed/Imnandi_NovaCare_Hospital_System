using Imnandi_NovaCare_Hospital_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class CreatePatientViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required(ErrorMessage ="Id Number is required")]
        [StringLength(13)]
        public string IdNumber { get;  set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }
        public string CurrentMedication { get; set; }

        [Required]
        [StringLength(50)]
        public string BankName { get; set; }

        [Required]
        [StringLength(50)]
        public string BankAccountNumber { get; set; }

        public bool IsInsured { get; set; }
        public string InsuranceProvider { get; set; }

        public string PolicyNumber { get; set; }

        public string MedicalAidPlan { get; set; }

        public string MainMemberName { get; set; }
        public string MedicalHistoryDescription { get; set; }
        public string Allergies { get; set; }
    }
}

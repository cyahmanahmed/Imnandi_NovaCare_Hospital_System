using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class CreateEmployeeViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName is required")]
        [StringLength(50)]
        [Display(Name = "LastName")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Tax Number")]
        public string TaxNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Bank Account Number")]
        public string BankAccountNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        public int? WardId { get; set; } 
        public IEnumerable<SelectListItem> Wards { get; set; } = new List<SelectListItem>();

    }
}

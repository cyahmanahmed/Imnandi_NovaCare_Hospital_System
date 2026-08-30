using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.ViewModels
{
    public class CreateFirstAdminViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string ?UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm the password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        [StringLength(10, ErrorMessage = "Phone number cannot be longer than 10 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }
    }
}
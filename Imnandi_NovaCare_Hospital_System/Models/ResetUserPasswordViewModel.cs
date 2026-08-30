using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ResetUserPasswordViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "User Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
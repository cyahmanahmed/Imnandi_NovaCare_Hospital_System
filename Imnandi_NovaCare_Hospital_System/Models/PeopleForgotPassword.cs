using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class PeopleForgotPassword
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public bool IsHandled { get; set; } = false;
    }
}

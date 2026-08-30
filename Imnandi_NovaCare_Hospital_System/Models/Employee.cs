using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100)]
        public string? JobTitle { get; set; }  
        public string? Department { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; }
        public string? EmergencyContact { get; set; }
        [Required]
        public string? Email { get; set; }

        [Required]
        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(20)]
        public string? TaxNumber { get; set; }

        [StringLength(50)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual Doctor Doctor { get; set; }
        public virtual Nurse Nurse { get; set; }
        public virtual NurseSister NurseSister { get; set; }
        public virtual ScriptManager ScriptManager { get; set; }
        public virtual StockManager StockManager { get; set; }
        public virtual Admin Admin { get; set; }
        public virtual WardAdmin WardAdmin { get; set; }

    }
}

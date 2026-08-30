using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Admin
    {
        public int AdminId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsAvail { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string Department { get; set; } = "Administration";

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class StockManager
    {
        [Key]
        public int StockManagerId {  get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Department { get; set; }
        public bool IsAvail { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
        public Collection<Order>Orders { get; set; } = new();
        public Collection<StockTake> StockTake { get; set; } = new();
    }
}

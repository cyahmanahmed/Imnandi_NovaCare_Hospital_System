using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(150)]
        public string SupplierName { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; }

        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public Collection<Order> Orders { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [ForeignKey("Consumable")]
        public int ConsumableId { get; set; }
        public Consumable Consumable { get; set; }

        public int QuantityRequested { get; set; }
        public int? QuantityReceived { get; set; }
        public bool IsDeleted { get; set; }
    }
}

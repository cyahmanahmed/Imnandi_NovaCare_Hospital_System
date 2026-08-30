using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{   
    public class WardStock
    {
        [Key]
        public int WardStockId { get; set; }

        public int WardId { get; set; }
        public Ward Ward { get; set; }

        public int ConsumableId { get; set; }
        public Consumable Consumable { get; set; }

        public int QuantityInWard { get; set; }
    }
}

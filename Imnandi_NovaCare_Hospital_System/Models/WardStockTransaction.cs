using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardStockTransaction
    {
        [Key]
        public int WardStockTransactionId { get; set; }

        public int WardId { get; set; }
        public Ward Ward { get; set; }

        public int ConsumableId { get; set; }
        public Consumable Consumable { get; set; }

        public int Quantity { get; set; }

        public DateTime DateReceived { get; set; } = DateTime.Now;

        public string TransactionType { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

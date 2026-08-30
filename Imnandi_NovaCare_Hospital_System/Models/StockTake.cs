using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class StockTake
    {
        [Key]
        public int StockTakeId { get; set; }

        public DateOnly Date { get; set; }
        public int QuantityCountered { get; set; }

        public int StockManagerId { get; set; }
        public StockManager StockManager { get; set; }

        public Collection<Consumable> Consumables { get; set; } = new();

        public int? WardId { get; set; } 
        public Ward Ward { get; set; }   
        public bool IsDeleted { get; internal set; }
    }
}

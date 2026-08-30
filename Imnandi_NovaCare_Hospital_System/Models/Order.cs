using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Order
    {
        [Key] 
        public int OrderId {  get; set; }
        public string OrderNumber {  get; set; }
        public string OrderName { get; set; }   
        public string Description { get; set; }
        public DateOnly Date { get; set; }
        public bool IsReceived { get; set; } = false;

        public ICollection<OrderItem> OrderItems { get; set; }
        public Collection<Consumable> Consumables { get; set; } = new();

        public int StockManagerId { get; set; }
        public StockManager StockManager { get; set; }
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int HospitalStoreId { get; set; }
        public HospitalStore HospitalStore { get; set; }

        public int? WardId { get; set; }
        public Ward Ward { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

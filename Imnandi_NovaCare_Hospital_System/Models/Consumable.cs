using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Consumable
    {
        [Key]
        public int ConsumableId {  get; set; }
        [Required]
        public string ConsumableName { get; set; }
        
        public int QuantityOnHand { get; set; }
        
        
        [Required]
        public string Description { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string Unit {  get; set; }
     
        public bool IsDeleted {  get; set; }
        public int MinimumConsumables { get; set; } = 5;


        public int? HospitalStoreId {  get; set; }
        public HospitalStore HospitalStore { get; set; }

        public int? OrderId {  get; set; }
        public Order Order { get; set; }


        public int? StockTakeId {  get; set; }
        public StockTake StockTake { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}

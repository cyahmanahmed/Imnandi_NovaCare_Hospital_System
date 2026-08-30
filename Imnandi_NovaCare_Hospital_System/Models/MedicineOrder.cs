using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class MedicineOrder
    {
        [Key]
        public int MedicineOrderId { get; set; }

        [Required]
        public string OrderNumber { get; set; }

        [Required]
        public string OrderName { get; set; }

        public string Description { get; set; }

        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public bool IsReceived { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public int StockManagerId { get; set; }
        public StockManager StockManager { get; set; }

        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int HospitalStoreId { get; set; }
        public HospitalStore HospitalStore { get; set; }

        public ICollection<MedicineOrderItem> MedicineOrderItems { get; set; }
            = new List<MedicineOrderItem>();
    }
}

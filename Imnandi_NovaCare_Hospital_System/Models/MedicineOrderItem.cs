using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class MedicineOrderItem
    {
        [Key]
        public int MedicineOrderItemId { get; set; }

        public int MedicineOrderId { get; set; }
        public MedicineOrder MedicineOrder { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

        public int QuantityRequested { get; set; }

        public int QuantityReceived { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;
    }
}

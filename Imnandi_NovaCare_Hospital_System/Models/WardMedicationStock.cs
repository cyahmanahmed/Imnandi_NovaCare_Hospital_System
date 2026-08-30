using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardMedicationStock
    {
        [Key]
        public int WardMedicationStockId { get; set; }

        public int WardId { get; set; }
        public Ward Ward { get; set; }

        public int MedicationId { get; set; }
        public Medication Medication { get; set; }

        public int QuantityInWard { get; set; }
    }
}
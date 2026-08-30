using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ScriptPrescriptionMedication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScriptPrescriptionId { get; set; }
        [ForeignKey("ScriptPrescriptionId")]
        public ScriptPrescription ScriptPrescription { get; set; }

        [Required]
        public int MedicationId { get; set; }
        [ForeignKey("MedicationId")]
        public Medication Medication { get; set; }
        public int Quantity { get; set; }
        public int QuantityReceived { get; set; } = 0;
    }
}

using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ScriptPrescription
    {
        [Key]
        public int ScriptPrescriptionId { get; set; }

        [Required]
        public int PrescriptionId { get; set; } 
        [ForeignKey("PrescriptionId")]
        public Prescription Prescription { get; set; } = null!; 

        [Required]
        public int ScriptManagerId { get; set; }
        [ForeignKey("ScriptManagerId")]
        public ScriptManager ScriptManager { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; 
        public DateTime? ProcessedDate { get; set; } 
        public DateTime? ReceivedDate { get; set; }  
        public string Notes { get; set; }
        public bool IsVerified { get; set; } = false;
        public string ReceivedFromId { get; set; } 
        public User ReceivedFrom { get; set; }
        public ICollection<ScriptPrescriptionMedication> Medications { get; set; } = new List<ScriptPrescriptionMedication>();
        public DateTime VerifiedDate { get; internal set; }
        public string VerifiedBy { get; set; }
        public DateTime AssignedDate { get; internal set; }
        public bool IsDeleted { get;  set; }= false;
    }
}

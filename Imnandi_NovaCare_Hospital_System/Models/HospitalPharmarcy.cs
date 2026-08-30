using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class HospitalPharmarcy
    {
        [Key]
        public int HospitalPharmacyId {  get; set; }
        [Required, MaxLength(150)]
        public string HospitalPharmarcyName {  get; set; }

        [MaxLength(250)]
        public string Location { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; }
        public bool IsDeleted { get; set; }=false;
    }
}

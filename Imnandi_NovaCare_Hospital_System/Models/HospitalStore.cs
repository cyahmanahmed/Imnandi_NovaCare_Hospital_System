using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class HospitalStore
    {
        [Key]
        public int HospitalStoreId {  get; set; }
        public string HospitalStoreName { get; set; }
        public string Location { get; set; }

        public Collection<Order> Orders { get; set; } = new();
        public Collection<Consumable> Consumables { get; set; } = new();
        public bool IsDeleted { get; set; } = false;
    }
}

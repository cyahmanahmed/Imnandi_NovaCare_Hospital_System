using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Ward
    {
        [Key]
        public int WardId {  get; set; }
        [Required(ErrorMessage = "Ward name is required")]
        [StringLength(100, ErrorMessage = "Ward name cannot exceed 100 characters")]
        public string WardName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Location (e.g. Floor 2, East Wing) cannot exceed 100 characters")]
        public string Location {  get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Total capacity (beds) is required")]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100 beds")]
        public int Capacity { get; set; }



        public bool IsDeleted { get; set; } = false;
        public Collection<Room> Rooms { get; set; }
        public Collection<Nurse> Nurses { get; set; }
        public int? NurseSisterId {  get; set; }
        public NurseSister NurseSister { get; set; }


        public Collection<StockTake> StockTakes { get; set; } = new(); //Ward can have stock takes
        public Collection<Order> Orders { get; set; } = new(); //Wards can place orders
        public Collection<WardStock> WardStocks { get; set; } = new();
        public ICollection<PatientHistory> PatientHistories { get; set; }
        public ICollection<WardNurseAssignment> NurseAssignments { get; set; } = new List<WardNurseAssignment>();
    }
}

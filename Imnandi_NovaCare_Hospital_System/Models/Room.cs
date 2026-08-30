using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Room
    {

        [Key]
        public int RoomId {  get; set; }
        [Required]
        public string RoomNumber {  get; set; }
        [Required]
        public string RoomName {  get; set; }
        [Required]
        public int RoomCapacity {  get; set; }
        public int NoOccupiedBed { get; set; }= 0;
        public int AvailableBeds => RoomCapacity-NoOccupiedBed;
        public bool IsDeleted { get; set; }=false;



        public int WardId {  get; set; }
        public Ward Ward { get; set; }

        public Collection<Bed>Beds { get; set; }
        public ICollection<PatientHistory> PatientHistories { get; set; }
    }
}

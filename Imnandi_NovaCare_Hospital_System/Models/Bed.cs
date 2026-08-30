using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Bed
    {
        [Key]
        public int BedId {  get; set; }
        [Required]
        public string BedNumber {  get; set; }
        [Required]
        public string BedType { get; set; }
        public bool IsOccupied { get; set; }=false;
        public bool IsDeleted { get; set; }=false;



        public int RoomId {  get; set; }
        public Room Room { get; set; }

        public ICollection<PatientHistory> PatientHistories { get; set; }
    }
}

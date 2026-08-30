using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardNurseAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WardId { get; set; }

        [ForeignKey(nameof(WardId))]
        public Ward Ward { get; set; } = null!;

        [Required]
        public int NurseSisterId { get; set; }

        [ForeignKey(nameof(NurseSisterId))]
        public NurseSister NurseSister { get; set; } = null!;

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UnassignedDate { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ManageWardViewModel
    {
        public int WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;

        public int? NurseSisterId { get; set; }
        public string NurseSisterName { get; set; } = "Unassigned";
        public List<Nurse> Nurses { get; set; } = new();
        public IEnumerable<SelectListItem> NurseSisters { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}

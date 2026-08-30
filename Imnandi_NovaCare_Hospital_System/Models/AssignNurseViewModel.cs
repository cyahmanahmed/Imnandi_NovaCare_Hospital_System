using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class AssignNurseViewModel
    {
        [Required(ErrorMessage = "Please select a nurse")]
        [Display(Name = "Nurse")]
        public int SelectedNurseId { get; set; }

        public List<Nurse> Nurses { get; set; } = new List<Nurse>();

        [Display(Name = "Ward")]
        public int? SelectedWardId { get; set; }

        public List<Ward> Wards { get; set; } = new List<Ward>();

        [Display(Name = "Patient")]
        public int? SelectedPatientId { get; set; }

        [Display(Name = "Patients")]
        public List<int> SelectedPatientIds { get; set; } = new List<int>();

        public List<Patient> Patients { get; set; } = new List<Patient>();
        public List<Nurse> AllNursesWithPatients { get; set; } = new List<Nurse>();
    }
}

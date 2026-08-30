using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class Instruction
    {
        [Key]
        public int InstructionId {  get; set; }

        public string Description {  get; set; }
        public string Title {  get; set; }
        public bool isActive { get; set; } = false;




        public int DoctorId {  get; set; }
        public Doctor Doctor { get; set; }
        public int PatientId {  get; set; }
        public Patient Patient { get; set; }
        public int? NurseId {  get; set; }
        public Nurse Nurse { get; set; }
        public DateTime CreatedAt { get; internal set; }
    }
}

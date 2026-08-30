namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ManageAlertViewModel
    {
        public int AlertId { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RecipientType { get; set; } 
        public string TargetUserName { get; set; }
        public string TargetRole { get; set; } 
        public int ReceivedCount { get; set; }
        public bool IsActive { get; set; } 
        public bool? IsRead { get; set; } 
    }
}

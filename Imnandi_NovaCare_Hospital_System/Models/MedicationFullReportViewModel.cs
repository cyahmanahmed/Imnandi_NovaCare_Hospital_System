namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class MedicationFullReportViewModel
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string Description { get; set; }
        public int? QuantityOnHand { get; set; }
        public string DosageForm { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int OpeningStock { get; set; }
        public int TotalStockReceived { get; set; }
        public int TotalStockAvailable { get; set; }
        public int TotalStockUsed { get; set; }
        public int ClosingStock { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TotalScriptActions { get; set; }
        public int TotalAdministrations { get; set; }

        public List<MedicationStockRecord> StockReceived { get; set; } = new();
        public List<PrescriptionRecord> Prescriptions { get; set; } = new();
        public List<ScriptPrescriptionRecord> ScriptActions { get; set; } = new();
        public List<AdministrationRecord> Administrations { get; set; } = new();
    }
    public class MedicationStockRecord
    {
        public int MedicineOrderItemId { get; set; }

        public int MedicineOrderId { get; set; }

        public string OrderNumber { get; set; }
        public string OrderName { get; set; }
        public DateOnly OrderDate { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int HospitalStoreId { get; set; }
        public string HospitalStoreName { get; set; }
        public int StockManagerId { get; set; }
        public string StockManagerName { get; set; }
        public int QuantityRequested { get; set; }
        public int QuantityReceived { get; set; }

        public bool IsReceived { get; set; }
    }
    public class PrescriptionRecord
    {
        public int PrescriptionId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorRole { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateOnly IssueDate { get; set; }
        public TimeOnly IssueTime { get; set; }
        public string Status { get; set; }
        public string DosageInstructions { get; set; }
        public int? Level { get; set; }
        public int? ScriptManagerId { get; set; }
        public string ScriptManagerName { get; set; }
    }

    public class ScriptPrescriptionRecord
    {
        public int ScriptPrescriptionId { get; set; }
        public int PrescriptionId { get; set; }
        public int? ScriptManagerId { get; set; }
        public string ScriptManagerName { get; set; }
        public string Status { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedFromUser { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }
    }

    public class AdministrationRecord
    {
        public int MedicationAdministrationId { get; set; }
        public int MedicationId { get; set; }
        public int PrescriptionId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int? NurseId { get; set; }
        public string NurseName { get; set; }
        public string NurseRole { get; set; }
        public int? NurseSisterId { get; set; }
        public string NurseSisterName { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateOnly DateAdministered { get; set; }
        public string Dosage { get; set; }
        public string Purpose { get; set; }
        public bool IsSeen { get; set; }
    }
}
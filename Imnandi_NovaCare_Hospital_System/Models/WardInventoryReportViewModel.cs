namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardInventoryReportViewModel
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }

        public string NurseSisterName { get; set; }

        public DateTime ReportDate { get; set; } = DateTime.Now;

        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public int TotalMedicationTypes { get; set; }
        public int TotalConsumableTypes { get; set; }

        public int TotalMedicationQuantityInWard { get; set; }
        public int TotalConsumableQuantityInWard { get; set; }

        public int TotalMedicationReceived { get; set; }
        public int TotalMedicationIssued { get; set; }
        public int TotalMedicationAdjusted { get; set; }

        public int TotalStockTakes { get; set; }
        public int TotalItemsCounted { get; set; }
        public int TotalShortages { get; set; }
        public int TotalSurpluses { get; set; }
        public int TotalMatchingItems { get; set; }

        public List<WardInventoryStockManagerRecord> StockManagers { get; set; } = new();

        public List<WardInventoryMedicationRecord> Medications { get; set; } = new();

        public List<WardInventoryConsumableRecord> Consumables { get; set; } = new();

        public List<WardInventoryStockTakeRecord> StockTakes { get; set; } = new();

        public List<WardInventoryMedicationTransactionRecord> MedicationTransactions { get; set; } = new();

        public List<WardInventoryPrescriptionRecord> Prescriptions { get; set; } = new();

        public List<WardInventoryMedicationAdministrationRecord> MedicationAdministrations { get; set; } = new();
    }

    public class WardInventoryStockManagerRecord
    {
        public int StockManagerId { get; set; }

        public string StockManagerName { get; set; }

        public string Department { get; set; }
    }

    public class WardInventoryMedicationRecord
    {
        public int MedicationId { get; set; }

        public string MedicationName { get; set; }

        public string Description { get; set; }

        public string DosageForm { get; set; }

        public string Manufacturer { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int QuantityInWard { get; set; }

        public int TotalReceived { get; set; }

        public int TotalIssued { get; set; }

        public int TotalAdjusted { get; set; }

        public int QuantityAtStartOfPeriod { get; set; }

        public int QuantityAtEndOfPeriod { get; set; }

        public string StockStatus { get; set; }
    }

    public class WardInventoryConsumableRecord
    {
        public int ConsumableId { get; set; }

        public string ConsumableName { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public DateOnly ExpiryDate { get; set; }

        public int SystemQuantity { get; set; }

        public int PhysicalQuantity { get; set; }

        public int Difference { get; set; }

        public string StockStatus { get; set; }

        public int MinimumConsumables { get; set; }

        public bool IsBelowMinimum { get; set; }

        public bool IsExpired { get; set; }
    }

    public class WardInventoryStockTakeRecord
    {
        public int StockTakeId { get; set; }

        public DateOnly Date { get; set; }

        public int StockManagerId { get; set; }

        public string StockManagerName { get; set; }

        public int QuantityCounted { get; set; }

        public int TotalConsumablesCounted { get; set; }

        public int TotalShortages { get; set; }

        public int TotalSurpluses { get; set; }

        public int TotalMatching { get; set; }

        public List<WardInventoryStockTakeConsumableRecord> Consumables { get; set; } = new();
    }

    public class WardInventoryStockTakeConsumableRecord
    {
        public int ConsumableId { get; set; }

        public string ConsumableName { get; set; }

        public string Unit { get; set; }

        public int SystemQuantity { get; set; }

        public int PhysicalQuantity { get; set; }

        public int Difference { get; set; }

        public string Status { get; set; }
    }

    public class WardInventoryMedicationTransactionRecord
    {
        public int TransactionId { get; set; }

        public int MedicationId { get; set; }

        public string MedicationName { get; set; }

        public int Quantity { get; set; }

        public DateTime Date { get; set; }

        public string TransactionType { get; set; }
    }

    public class WardInventoryPrescriptionRecord
    {
        public int PrescriptionId { get; set; }

        public int MedicationId { get; set; }

        public string MedicationName { get; set; }

        public string PatientName { get; set; }

        public string DoctorName { get; set; }

        public DateOnly IssueDate { get; set; }

        public TimeOnly IssueTime { get; set; }

        public string Status { get; set; }

        public string DosageInstructions { get; set; }

        public int Quantity { get; set; }

        public int? Level { get; set; }

        public string ScriptManagerName { get; set; }
    }

    public class WardInventoryMedicationAdministrationRecord
    {
        public int MedicationAdministrationId { get; set; }

        public int MedicationId { get; set; }

        public string MedicationName { get; set; }

        public string PatientName { get; set; }

        public string NurseName { get; set; }

        public string NurseSisterName { get; set; }

        public string DoctorName { get; set; }

        public DateOnly AdministrationDate { get; set; }

        public string Dosage { get; set; }

        public string Purpose { get; set; }

        public int PrescriptionId { get; set; }
    }
}
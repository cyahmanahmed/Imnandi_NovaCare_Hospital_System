namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardMedicationReportViewModel
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }

        public string StockManagerName { get; set; }
        public string Department { get; set; }

        public int TotalMedicationTypes { get; set; }
        public int TotalQuantityInWard { get; set; }
        public int TotalReceived { get; set; }
        public int TotalIssued { get; set; }
        public int TotalAdjusted { get; set; }

        public DateTime ReportGenerated { get; set; }

        public List<WardMedicationStockRecord> CurrentStock { get; set; } = new();
        public List<WardMedicationTransactionRecord> Transactions { get; set; } = new();
    }

    public class WardMedicationStockRecord
    {
        public int WardMedicationStockId { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string DosageForm { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int QuantityInWard { get; set; }
    }

    public class WardMedicationTransactionRecord
    {
        public int WardMedicationTransactionId { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string DosageForm { get; set; }
        public int Quantity { get; set; }
        public DateTime DateReceived { get; set; }
        public string TransactionType { get; set; }
    }
}

namespace Imnandi_NovaCare_Hospital_System.Models.Reports
{
    public class SupplierReportViewModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public int TotalOrders { get; set; }

        public int TotalConsumableOrders { get; set; }

        public int TotalMedicineOrders { get; set; }

        public int CompletedOrders { get; set; }

        public int PendingOrders { get; set; }

        public int TotalItemsOrdered { get; set; }

        public int TotalItemsReceived { get; set; }

        public int TotalItemsOutstanding { get; set; }

        public double OverallDeliveryRate { get; set; }
        public List<SupplierConsumableReportItem> ConsumableItems { get; set; } = new();

        public List<SupplierMedicineReportItem> MedicineItems { get; set; } = new();

        public List<SupplierOrderReportItem> Orders { get; set; }= new();
    }

    public class SupplierConsumableReportItem
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; }

        public string OrderName { get; set; }

        public DateOnly OrderDate { get; set; }

        public string ConsumableName { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public int QuantityRequested { get; set; }

        public int QuantityReceived { get; set; }

        public int QuantityOutstanding { get; set; }

        public bool IsReceived { get; set; }

        public string StockManagerName { get; set; }

        public string HospitalStoreName { get; set; }

        public string HospitalStoreLocation { get; set; }

        public string WardName { get; set; }
    }

    public class SupplierMedicineReportItem
    {
        public int MedicineOrderId { get; set; }

        public string OrderNumber { get; set; }

        public string OrderName { get; set; }

        public DateOnly OrderDate { get; set; }

        public string MedicationName { get; set; }

        public string Description { get; set; }

        public string DosageForm { get; set; }

        public string Manufacturer { get; set; }

        public int QuantityRequested { get; set; }

        public int QuantityReceived { get; set; }

        public int QuantityOutstanding { get; set; }

        public bool IsReceived { get; set; }

        public string StockManagerName { get; set; }

        public string HospitalStoreName { get; set; }

        public string HospitalStoreLocation { get; set; }
    }

    public class SupplierOrderReportItem
    {
        public int OrderId { get; set; }

        public bool IsMedicineOrder { get; set; }

        public string OrderNumber { get; set; }

        public string OrderName { get; set; }

        public string Description { get; set; }

        public DateOnly OrderDate { get; set; }

        public bool IsReceived { get; set; }

        public string Status { get; set; }

        public int TotalItems { get; set; }

        public int TotalQuantityRequested { get; set; }

        public int TotalQuantityReceived { get; set; }

        public int OutstandingQuantity { get; set; }

        public double DeliveryRate { get; set; }

        public string StockManagerName { get; set; }

        public string HospitalStoreName { get; set; }

        public string HospitalStoreLocation { get; set; }

        public string WardName { get; set; }
    }
}
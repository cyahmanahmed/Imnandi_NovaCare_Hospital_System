namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class StockManagerDashboardViewModel
    {
        public int StockManagerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public int TotalOrders { get; set; }
        public int PendingOrderCount { get; set; }
        public int CompletedOrderCount { get; set; }
        public int TotalConsumables { get; set; }

        public List<Order> PendingOrders { get; set; } = new();
        public List<Order> CompletedOrders { get; set; } = new();
        public List<Consumable> LowStockConsumables { get; set; } = new();
        public List<StockTake> RecentStockTakes { get; set; } = new();
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string UserName { get; set; }
    }
}

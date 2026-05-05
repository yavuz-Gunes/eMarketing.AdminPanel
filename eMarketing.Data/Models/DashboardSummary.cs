namespace eMarketing.Data.Models
{
    public class DashboardSummary
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }

        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }

        public int TotalOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingPaymentOrders { get; set; }

        public int TotalCustomers { get; set; }
        public int ActiveStores { get; set; }
        public int StaffCount { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}

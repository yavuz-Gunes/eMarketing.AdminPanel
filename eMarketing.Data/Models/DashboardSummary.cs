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
    }
}
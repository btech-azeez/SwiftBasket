namespace Zepto.Backend.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public List<OrderDto> RecentOrders { get; set; } = new();
    }
}

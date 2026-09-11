namespace MarketplaceManagerWeb.Models
{
    public class ManagerReportDto
    {
        public string ManagerName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalLogistics { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
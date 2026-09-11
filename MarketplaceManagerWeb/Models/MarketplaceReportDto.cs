namespace MarketplaceManagerWeb.Models
{
    public class MarketplaceReportDto
    {
        public string MarketplaceName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalLogistics { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
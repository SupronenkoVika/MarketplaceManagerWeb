namespace MarketplaceManagerWeb.Models
{
    public class ProductReportDto
    {
        public string ProductArticle { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalLogistics { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
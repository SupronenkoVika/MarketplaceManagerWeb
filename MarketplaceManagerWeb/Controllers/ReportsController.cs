using MarketplaceManagerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Отчет по менеджерам
        [HttpPost]
        public async Task<IActionResult> Managers(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ManagerID, s.Manager.ManagerFName, s.Manager.ManagerLName })
                .Select(g => new
                {
                    ManagerId = g.Key.ManagerID,
                    ManagerName = $"{g.Key.ManagerFName} {g.Key.ManagerLName}",
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Менеджеры";

            return View("ReportResult", report);
        }

        // Отчет по маркетплейсам
        [HttpPost]
        public async Task<IActionResult> Marketplaces(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.MarketplaceID, s.Marketplace.MarketplaceName, s.Marketplace.CommissionRate })
                .Select(g => new
                {
                    MarketplaceId = g.Key.MarketplaceID,
                    MarketplaceName = g.Key.MarketplaceName,
                    CommissionRate = g.Key.CommissionRate,
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Маркетплейсы";

            return View("ReportResult", report);
        }

        // Отчет по товарам
        [HttpPost]
        public async Task<IActionResult> Products(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ProductID, s.Product.ProductArticle, s.Product.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductID,
                    ProductArticle = g.Key.ProductArticle,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Товары";

            return View("ReportResult", report);
        }
    }
}
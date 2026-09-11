using MarketplaceManagerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Общие показатели
            var totalSales = await _context.Sales.CountAsync();
            var totalRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var totalProfit = await _context.Sales.SumAsync(s => (decimal?)s.NetProfit) ?? 0;
            var totalCommission = await _context.Sales.SumAsync(s => (decimal?)s.Commission) ?? 0;

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalProfit = totalProfit;
            ViewBag.TotalCommission = totalCommission;

            // Продажи по маркетплейсам (для круговой диаграммы)
            var salesByMarketplace = await _context.Sales
                .GroupBy(s => s.Marketplace.MarketplaceName)
                .Select(g => new
                {
                    Marketplace = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            ViewBag.MarketplaceLabels = salesByMarketplace.Select(x => x.Marketplace).ToArray();
            ViewBag.MarketplaceCounts = salesByMarketplace.Select(x => x.Count).ToArray();
            ViewBag.MarketplaceRevenues = salesByMarketplace.Select(x => x.Revenue).ToArray();

            // Динамика продаж за последние 30 дней
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var dailySales = await _context.Sales
                .Where(s => s.SaleDate >= thirtyDaysAgo)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.TotalAmount),
                    Profit = g.Sum(s => s.NetProfit)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.DailyLabels = dailySales.Select(x => x.Date.ToString("dd.MM")).ToArray();
            ViewBag.DailyRevenues = dailySales.Select(x => x.Revenue).ToArray();
            ViewBag.DailyProfits = dailySales.Select(x => x.Profit).ToArray();

            // Последние 10 продаж
            var recentSales = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.Marketplace)
                .Include(s => s.Manager)
                .OrderByDescending(s => s.SaleDate)
                .Take(10)
                .ToListAsync();

            return View(recentSales);
        }
    }
}
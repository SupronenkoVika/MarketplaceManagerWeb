using MarketplaceManagerWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Общие финансовые показатели
            var totalSales = await _context.Sales.CountAsync();
            var totalRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var totalProfit = await _context.Sales.SumAsync(s => (decimal?)s.NetProfit) ?? 0;

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalProfit = totalProfit;

            // 2. Динамика продаж по дням (в ШТУКАХ) за последние 30 дней
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var dailySalesQuantities = await _context.Sales
                .Where(s => s.SaleDate >= thirtyDaysAgo)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Sum(s => s.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.DailyQtyLabels = dailySalesQuantities.Select(x => x.Date.ToString("dd.MM")).ToArray();
            ViewBag.DailyQuantities = dailySalesQuantities.Select(x => x.Quantity).ToArray();

            // 3. Продажи по маркетплейсам (в ШТУКАХ)
            var marketplaceSalesQuantities = await _context.Sales
                .GroupBy(s => s.Marketplace.MarketplaceName)
                .Select(g => new
                {
                    Marketplace = g.Key,
                    Quantity = g.Sum(s => s.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();

            ViewBag.MarketplaceQtyLabels = marketplaceSalesQuantities.Select(x => x.Marketplace).ToArray();
            ViewBag.MarketplaceQuantities = marketplaceSalesQuantities.Select(x => x.Quantity).ToArray();

            // 4. Последние 5 продаж для таблицы
            var recentSales = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.Marketplace)
                .Include(s => s.Manager)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToListAsync();

            return View(recentSales);
        }
    }
}
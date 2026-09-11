using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Manager)
                .Include(s => s.Product)
                .Include(s => s.Marketplace)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // GET: Sales/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.Marketplaces = await _context.Marketplaces
                .OrderBy(m => m.MarketplaceName)
                .ToListAsync();
            // Добавляем список менеджеров
            ViewBag.Managers = await _context.Managers
                .OrderBy(m => m.ManagerLName)
                .ThenBy(m => m.ManagerFName)
                .ToListAsync();

            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SaleDate,ManagerID,ProductID,MarketplaceID,Quantity")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                // Получаем данные о товаре, маркетплейсе и менеджере
                var product = await _context.Products.FindAsync(sale.ProductID);
                var marketplace = await _context.Marketplaces.FindAsync(sale.MarketplaceID);
                var managerIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(managerIdString) || !int.TryParse(managerIdString, out int managerId))
                {
                    return RedirectToAction("Login", "Auth");
                }
                sale.ManagerID = managerId;

                if (product == null || marketplace == null)
                {
                    ModelState.AddModelError("", "Товар или маркетплейс не найдены");
                    return View(sale);
                }

                // Проверяем наличие товара на складе
                if (product.Stock < sale.Quantity)
                {
                    ModelState.AddModelError("", $"Недостаточно товара на складе. Доступно: {product.Stock}");
                    return View(sale);
                }

                // Если дата не указана или некорректна, ставим текущую дату
                if (sale.SaleDate == default || sale.SaleDate.Year < 2000)
                {
                    sale.SaleDate = DateTime.Now;
                }
                else
                {
                    // Оставляем только дату, без времени, для чистоты данных
                    sale.SaleDate = sale.SaleDate.Date;
                }

                // Рассчитываем финансовые показатели
                sale.TotalAmount = product.Price * sale.Quantity;
                sale.Commission = sale.TotalAmount * marketplace.CommissionRate / 100;
                sale.LogisticsCost = marketplace.LogisticsCost * sale.Quantity;
                sale.NetProfit = sale.TotalAmount - sale.Commission - (product.CostPrice * sale.Quantity) - sale.LogisticsCost;

                // Уменьшаем остаток товара
                product.Stock -= sale.Quantity;

                // Сохраняем в базу данных
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Если есть ошибки валидации, возвращаем форму с данными
            ViewBag.Products = await _context.Products
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.Marketplaces = await _context.Marketplaces
                .OrderBy(m => m.MarketplaceName)
                .ToListAsync();

            ViewBag.Managers = await _context.Managers
                .OrderBy(m => m.ManagerLName)
                .ThenBy(m => m.ManagerFName)
                .ToListAsync();

            return View(sale);
        }
    }
}
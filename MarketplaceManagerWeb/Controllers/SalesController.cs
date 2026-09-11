using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
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

            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,MarketplaceID,Quantity")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                // Получаем данные о товаре и маркетплейсе
                var product = await _context.Products.FindAsync(sale.ProductID);
                var marketplace = await _context.Marketplaces.FindAsync(sale.MarketplaceID);

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

                // Заглушка для менеджера (в будущем будет из сессии)
                var manager = await _context.Managers.FirstAsync();

                // Рассчитываем финансовые показатели
                sale.SaleDate = DateTime.Now;
                sale.ManagerID = manager.ManagerID;
                sale.TotalAmount = product.Price * sale.Quantity;
                sale.Commission = sale.TotalAmount * marketplace.CommissionRate / 100;
                sale.NetProfit = sale.TotalAmount - sale.Commission;

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

            return View(sale);
        }
    }
}
using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.ProductArticle.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
            return View(await products.OrderBy(p => p.ProductName).ToListAsync());
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ProductArticle,ProductName,Price,Stock")] Product product)
        {
            // Проверка уникальности артикула
            var exists = await _context.Products
                .AnyAsync(p => p.ProductArticle == product.ProductArticle);
            if (exists)
            {
                ModelState.AddModelError("ProductArticle", "Товар с таким артикулом уже существует");
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductArticle,ProductName,Price,CostPrice,Stock")] Product product)
        {
            if (id != product.ProductID) return NotFound();

            // Проверка уникальности артикула
            var exists = await _context.Products
                .AnyAsync(p => p.ProductArticle == product.ProductArticle && p.ProductID != id);
            if (exists)
            {
                ModelState.AddModelError("ProductArticle", "Товар с таким артикулом уже существует");
            }

            if (ModelState.IsValid)
            {
                // 1. Получаем текущий товар из БД (EF Core начинает его отслеживать)
                var existingProduct = await _context.Products.FindAsync(id);

                if (existingProduct != null)
                {
                    // 2. Проверяем, изменились ли цена или себестоимость
                    bool priceChanged = existingProduct.Price != product.Price;
                    bool costPriceChanged = existingProduct.CostPrice != product.CostPrice;

                    // 3. Если что-то изменилось - создаем запись в истории
                    if (priceChanged || costPriceChanged)
                    {
                        var managerIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        int.TryParse(managerIdString, out int managerId);

                        var historyRecord = new PriceHistory
                        {
                            ProductID = id,
                            OldPrice = existingProduct.Price,
                            NewPrice = product.Price,
                            OldCostPrice = existingProduct.CostPrice,
                            NewCostPrice = product.CostPrice,
                            ChangedDate = DateTime.Now,
                            ChangedByManagerID = managerId > 0 ? managerId : null
                        };
                        _context.PriceHistory.Add(historyRecord);
                    }

                    // 4. Обновляем свойства у УЖЕ ОТСЛЕЖИВАЕМОГО объекта (вместо _context.Update)
                    existingProduct.ProductArticle = product.ProductArticle;
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Price = product.Price;
                    existingProduct.CostPrice = product.CostPrice;
                    existingProduct.Stock = product.Stock;
                }

                // 5. Сохраняем все изменения (и товар, и историю)
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Проверяем, есть ли продажи с этим товаром
                var hasSales = await _context.Sales.AnyAsync(s => s.ProductID == id);
                if (hasSales)
                {
                    TempData["Error"] = "Нельзя удалить товар, по которому есть продажи";
                    return RedirectToAction(nameof(Index));
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/PriceHistory/5
        public async Task<IActionResult> PriceHistory(int? id)
        {
            if (id == null) return NotFound();

            var history = await _context.PriceHistory
                .Include(h => h.ChangedByManager)
                .Where(h => h.ProductID == id)
                .OrderByDescending(h => h.ChangedDate)
                .ToListAsync();

            var product = await _context.Products.FindAsync(id);
            ViewBag.ProductName = product?.ProductName;

            return View(history);
        }
    }
}
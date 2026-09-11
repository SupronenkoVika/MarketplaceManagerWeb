using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductArticle,ProductName,Price,Stock")] Product product)
        {
            if (id != product.ProductID) return NotFound();

            var exists = await _context.Products
                .AnyAsync(p => p.ProductArticle == product.ProductArticle && p.ProductID != id);
            if (exists)
            {
                ModelState.AddModelError("ProductArticle", "Товар с таким артикулом уже существует");
            }

            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
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
    }
}
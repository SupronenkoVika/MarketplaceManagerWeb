using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    public class MarketplacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarketplacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Marketplaces.OrderBy(m => m.MarketplaceName).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MarketplaceName,CommissionRate")] Marketplace marketplace)
        {
            var exists = await _context.Marketplaces
                .AnyAsync(m => m.MarketplaceName == marketplace.MarketplaceName);
            if (exists)
            {
                ModelState.AddModelError("MarketplaceName", "Маркетплейс с таким названием уже существует");
            }

            if (ModelState.IsValid)
            {
                _context.Marketplaces.Add(marketplace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marketplace);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace == null) return NotFound();
            return View(marketplace);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MarketplaceID,MarketplaceName,CommissionRate")] Marketplace marketplace)
        {
            if (id != marketplace.MarketplaceID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(marketplace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marketplace);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace == null) return NotFound();
            return View(marketplace);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var marketplace = await _context.Marketplaces.FindAsync(id);
            if (marketplace != null)
            {
                var hasSales = await _context.Sales.AnyAsync(s => s.MarketplaceID == id);
                if (hasSales)
                {
                    TempData["Error"] = "Нельзя удалить маркетплейс, по которому есть продажи";
                    return RedirectToAction(nameof(Index));
                }
                _context.Marketplaces.Remove(marketplace);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
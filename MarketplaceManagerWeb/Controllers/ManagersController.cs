using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Helpers;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Доступ только для администраторов
    public class ManagersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Managers
        public async Task<IActionResult> Index()
        {
            var managers = await _context.Managers
                .OrderBy(m => m.ManagerLName)
                .ThenBy(m => m.ManagerFName)
                .ToListAsync();
            return View(managers);
        }

        // GET: Managers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Managers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ManagerLName,ManagerFName,ManagerMName,ManagerLog,ManagerPass,ManagerIsAdmin")] Manager manager)
        {
            // Проверка уникальности логина
            var loginExists = await _context.Managers.AnyAsync(m => m.ManagerLog == manager.ManagerLog);
            if (loginExists)
            {
                ModelState.AddModelError("ManagerLog", "Менеджер с таким логином уже существует");
            }

            // Проверка минимальной длины пароля
            if (string.IsNullOrEmpty(manager.ManagerPass) || manager.ManagerPass.Length < 4)
            {
                ModelState.AddModelError("ManagerPass", "Пароль должен быть не менее 4 символов");
            }

            if (ModelState.IsValid)
            {
                // Хешируем пароль перед сохранением
                manager.ManagerPass = PasswordHelper.Hash(manager.ManagerPass);
                _context.Managers.Add(manager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(manager);
        }

        // GET: Managers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers.FindAsync(id);
            if (manager == null) return NotFound();

            return View(manager);
        }

        // POST: Managers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ManagerID,ManagerLName,ManagerFName,ManagerMName,ManagerLog,ManagerPass,ManagerIsAdmin")] Manager manager)
        {
            if (id != manager.ManagerID) return NotFound();

            // Проверка уникальности логина (исключая текущего менеджера)
            var loginExists = await _context.Managers.AnyAsync(m => m.ManagerLog == manager.ManagerLog && m.ManagerID != id);
            if (loginExists)
            {
                ModelState.AddModelError("ManagerLog", "Менеджер с таким логином уже существует");
            }

            if (ModelState.IsValid)
            {
                // Если пароль изменен, хешируем его
                if (!string.IsNullOrEmpty(manager.ManagerPass))
                {
                    manager.ManagerPass = PasswordHelper.Hash(manager.ManagerPass);
                }
                else
                {
                    // Если пароль не указан, оставляем старый
                    var existingManager = await _context.Managers.FindAsync(id);
                    manager.ManagerPass = existingManager.ManagerPass;
                }

                _context.Update(manager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(manager);
        }

        // GET: Managers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var manager = await _context.Managers
                .Include(m => m.Sales)
                .FirstOrDefaultAsync(m => m.ManagerID == id);

            if (manager == null) return NotFound();

            // Проверяем, есть ли продажи у этого менеджера
            ViewBag.HasSales = manager.Sales.Any();

            return View(manager);
        }

        // POST: Managers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manager = await _context.Managers.FindAsync(id);
            if (manager != null)
            {
                // Проверяем, есть ли продажи
                var hasSales = await _context.Sales.AnyAsync(s => s.ManagerID == id);
                if (hasSales)
                {
                    TempData["Error"] = "Нельзя удалить менеджера, у которого есть продажи в системе";
                    return RedirectToAction(nameof(Index));
                }

                _context.Managers.Remove(manager);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
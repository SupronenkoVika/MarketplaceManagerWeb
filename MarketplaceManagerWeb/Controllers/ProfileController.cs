using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize] // Доступно любому авторизованному пользователю
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // 1. Проверка совпадения новых паролей
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Новый пароль и подтверждение не совпадают";
                return View();
            }

            // 2. Проверка минимальной длины
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                ViewBag.Error = "Новый пароль должен быть не менее 4 символов";
                return View();
            }

            // 3. Получаем ID текущего пользователя из Cookie
            var managerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerIdString) || !int.TryParse(managerIdString, out int managerId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 4. Ищем пользователя в базе
            var manager = await _context.Managers.FindAsync(managerId);
            if (manager == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 5. Проверяем старый пароль
            var oldPasswordHash = PasswordHelper.Hash(oldPassword);
            if (manager.ManagerPass != oldPasswordHash)
            {
                ViewBag.Error = "Неверный текущий пароль";
                return View();
            }

            // 6. Обновляем пароль на новый (хешируем его)
            manager.ManagerPass = PasswordHelper.Hash(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Пароль успешно изменен!";
            return View();
        }
    }
}
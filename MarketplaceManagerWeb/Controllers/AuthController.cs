using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketplaceManagerWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Если пользователь уже авторизован, сразу отправляем его на главную панель
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string login, string password)
        {
            // Хешируем введенный пароль
            var passwordHash = PasswordHelper.Hash(password);

            // ОТЛАДКА: выводим в консоль что происходит
            System.Diagnostics.Debug.WriteLine($"========== ОТЛАДКА АВТОРИЗАЦИИ ==========");
            System.Diagnostics.Debug.WriteLine($"Введен логин: {login}");
            System.Diagnostics.Debug.WriteLine($"Введен пароль: {password}");
            System.Diagnostics.Debug.WriteLine($"Сгенерированный хеш: {passwordHash}");
            System.Diagnostics.Debug.WriteLine($"Длина хеша: {passwordHash.Length} символов");

            // Ищем менеджера в базе
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.ManagerLog == login && m.ManagerPass == passwordHash);

            if (manager == null)
            {
                // Дополнительная проверка: может быть, логин не найден?
                var userExists = await _context.Managers.AnyAsync(m => m.ManagerLog == login);

                System.Diagnostics.Debug.WriteLine($"Пользователь с логином '{login}' существует: {userExists}");

                if (!userExists)
                {
                    ViewBag.Error = $"Пользователь с логином '{login}' не найден";
                }
                else
                {
                    // Показываем хеш из базы для сравнения
                    var userFromDb = await _context.Managers.FirstOrDefaultAsync(m => m.ManagerLog == login);
                    System.Diagnostics.Debug.WriteLine($"Хеш пароля в базе: {userFromDb.ManagerPass}");
                    System.Diagnostics.Debug.WriteLine($"Длина хеша в базе: {userFromDb.ManagerPass.Length}");
                    System.Diagnostics.Debug.WriteLine($"Хеши СОВПАДАЮТ: {passwordHash == userFromDb.ManagerPass}");

                    ViewBag.Error = "Неверный пароль";
                }

                System.Diagnostics.Debug.WriteLine($"========== КОНЕЦ ОТЛАДКИ ==========");
                return View();
            }

            System.Diagnostics.Debug.WriteLine($"========== УСПЕШНЫЙ ВХОД ==========");
            System.Diagnostics.Debug.WriteLine($"========== КОНЕЦ ОТЛАДКИ ==========");

            // Создаем claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, manager.ManagerID.ToString()),
        new Claim(ClaimTypes.Name, $"{manager.ManagerLName} {manager.ManagerFName}"),
        new Claim(ClaimTypes.Role, manager.ManagerIsAdmin ? "Admin" : "Manager")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Dashboard");
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Удаляем "печеньку" (разлогиниваем пользователя)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Возвращаем на страницу входа
            return RedirectToAction("Login");
        }
    }
}
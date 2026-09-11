using MarketplaceManagerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Components
{
    // Имя класса должно заканчиваться на "ViewComponent"
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        // Метод InvokeAsync вызывается автоматически при отображении компонента
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Ищем товары, у которых остаток 5 или меньше
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock) // Сначала самые критичные (где 0)
                .Take(10) // Берем только топ-10, чтобы не перегружать меню
                .ToListAsync();

            // Передаем список в представление
            return View(lowStockProducts);
        }
    }
}
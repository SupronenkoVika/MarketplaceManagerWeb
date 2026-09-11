using MarketplaceManagerWeb.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Настройка аутентификации (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
    });

// 3. Настройка авторизации
builder.Services.AddAuthorization();

// 4. Добавление сервисов MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Настройка конвейера обработки запросов (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ВАЖНО: Строгий порядок ниже!
app.UseRouting();             // Сначала определяем маршрут
app.UseAuthentication();      // Потом проверяем, кто это (аутентификация)
app.UseAuthorization();       // Потом проверяем, есть ли у него права (авторизация)

// 6. Маршрутизация контроллеров (сделаем Auth стартовой страницей)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
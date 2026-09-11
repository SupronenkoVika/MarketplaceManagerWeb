using ClosedXML.Excel;
using MarketplaceManagerWeb.Data;
using MarketplaceManagerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceManagerWeb.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // HTML ОТЧЕТЫ (Для отображения в браузере)
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> Managers(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ManagerID, s.Manager.ManagerFName, s.Manager.ManagerLName })
                .Select(g => new
                {
                    ManagerName = $"{g.Key.ManagerFName} {g.Key.ManagerLName}",
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Менеджеры";

            return View("ReportResult", report);
        }

        [HttpPost]
        public async Task<IActionResult> Marketplaces(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.MarketplaceID, s.Marketplace.MarketplaceName, s.Marketplace.CommissionRate })
                .Select(g => new
                {
                    MarketplaceName = g.Key.MarketplaceName,
                    CommissionRate = g.Key.CommissionRate,
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Маркетплейсы";

            return View("ReportResult", report);
        }

        [HttpPost]
        public async Task<IActionResult> Products(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ProductID, s.Product.ProductArticle, s.Product.ProductName })
                .Select(g => new
                {
                    ProductArticle = g.Key.ProductArticle,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ReportType = "Товары";

            return View("ReportResult", report);
        }

        // ==========================================
        // ЭКСПОРТ В EXCEL
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> ExportManagers(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ManagerID, s.Manager.ManagerFName, s.Manager.ManagerLName })
                .Select(g => new ManagerReportDto
                {
                    ManagerName = $"{g.Key.ManagerFName} {g.Key.ManagerLName}",
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            return CreateExcelFile(
                report,
                "Отчет_по_менеджерам",
                new[] { "Менеджер", "Кол-во продаж", "Выручка", "Комиссии", "Логистика", "Чистая прибыль" },
                item => new object[] { item.ManagerName, item.SalesCount, item.TotalRevenue, item.TotalCommission, item.TotalLogistics, item.TotalProfit }
            );
        }

        [HttpPost]
        public async Task<IActionResult> ExportMarketplaces(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.MarketplaceID, s.Marketplace.MarketplaceName, s.Marketplace.CommissionRate })
                .Select(g => new MarketplaceReportDto
                {
                    MarketplaceName = g.Key.MarketplaceName,
                    CommissionRate = g.Key.CommissionRate,
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToListAsync();

            return CreateExcelFile(
                report,
                "Отчет_по_маркетплейсам",
                new[] { "Маркетплейс", "Комиссия %", "Кол-во продаж", "Выручка", "Комиссии", "Логистика", "Чистая прибыль" },
                item => new object[] { item.MarketplaceName, item.CommissionRate, item.SalesCount, item.TotalRevenue, item.TotalCommission, item.TotalLogistics, item.TotalProfit }
            );
        }

        [HttpPost]
        public async Task<IActionResult> ExportProducts(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Sales.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SaleDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(s => s.SaleDate <= endDate.Value.Date.AddDays(1));

            var report = await query
                .GroupBy(s => new { s.ProductID, s.Product.ProductArticle, s.Product.ProductName })
                .Select(g => new ProductReportDto
                {
                    ProductArticle = g.Key.ProductArticle,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.TotalAmount),
                    TotalCommission = g.Sum(s => s.Commission),
                    TotalLogistics = g.Sum(s => s.LogisticsCost),
                    TotalProfit = g.Sum(s => s.NetProfit)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            return CreateExcelFile(
                report,
                "Отчет_по_товарам",
                new[] { "Артикул", "Товар", "Кол-во продаж", "Общее кол-во", "Выручка", "Комиссии", "Логистика", "Чистая прибыль" },
                item => new object[] { item.ProductArticle, item.ProductName, item.SalesCount, item.TotalQuantity, item.TotalRevenue, item.TotalCommission, item.TotalLogistics, item.TotalProfit }
            );
        }

        // ==========================================
        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ГЕНЕРАЦИИ EXCEL
        // ==========================================

        private FileContentResult CreateExcelFile<T>(IEnumerable<T> data, string sheetName, string[] headers, Func<T, object[]> selector)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                // 1. Заголовки
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // 2. Данные
                int row = 2;
                foreach (var item in data)
                {
                    var values = selector(item);
                    for (int i = 0; i < values.Length; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = values[i]?.ToString() ?? "";
                    }
                    row++;
                }

                // 3. Строка ИТОГО
                row++;
                worksheet.Cell(row, 1).Value = "ИТОГО:";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                // Определяем, с какой колонки начинать суммирование (пропускаем текстовые и служебные)
                int firstNumericCol = 2;
                if (sheetName.Contains("менеджерам")) firstNumericCol = 3; // Пропускаем "Менеджер" и "Кол-во"
                if (sheetName.Contains("маркетплейсам")) firstNumericCol = 4; // Пропускаем "Маркетплейс", "%", "Кол-во"
                if (sheetName.Contains("товарам")) firstNumericCol = 5; // Пропускаем "Артикул", "Товар", "Кол-во", "Общее кол-во"

                for (int col = firstNumericCol; col <= headers.Length; col++)
                {
                    var sumFormula = $"=SUM({worksheet.Cell(2, col).Address}:{worksheet.Cell(row - 2, col).Address})";
                    worksheet.Cell(row, col).FormulaA1 = sumFormula;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                }

                // 4. Автоширина столбцов
                worksheet.Columns().AdjustToContents();

                // 5. Сохранение в поток и возврат файла
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"{sheetName}_{DateTime.Now:yyyy-MM-dd}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}
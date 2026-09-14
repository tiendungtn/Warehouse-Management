using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;

namespace QuanLyKho.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports/StockReport (Tồn kho)
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> StockReport()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.StockQuantity)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.TotalStockValue = products.Sum(p => p.StockQuantity * p.Price);
            return View(products);
        }

        // GET: Reports/RevenueReport (Doanh thu)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RevenueReport(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Invoices
                .Include(i => i.Issue)
                .AsQueryable();
            if (fromDate.HasValue)
            {
                query = query.Where(i => i.CreatedDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.CreatedDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var invoices = await query.OrderByDescending(i => i.CreatedDate).AsNoTracking().ToListAsync();

            ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(invoices);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportStockCsv()
        {
            var products = await _context.Products
             .Include(p => p.Category)
             .AsNoTracking()
             .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Mã hàng, Tên hàng, Danh mục, ĐVT, Đơn giá, Tồn kho, Tổng giá trị tồn");

            foreach (var p in products)
            {
                sb.AppendLine($"\"{p.ProductCode}\",\"{p.ProductName}\",\"{p.Category?.CategoryName}\",\"{p.Unit}\",{p.Price},{p.StockQuantity},{p.StockQuantity * p.Price}");
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fullBytes = new byte[preamble.Length + contentBytes.Length];
            Buffer.BlockCopy(preamble, 0, fullBytes, 0, preamble.Length);
            Buffer.BlockCopy(contentBytes, 0, fullBytes, preamble.Length, contentBytes.Length);

            return File(fullBytes, "text/csv;charset=utf-8", $"BaoCaoTonKho_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
    }
}

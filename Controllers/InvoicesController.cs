using System.ComponentModel;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuestDocument = QuestPDF.Fluent.Document;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuanLyKho.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Issue)
                .OrderByDescending(i => i.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
            return View(invoices);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Issue!)
                    .ThenInclude(issue => issue.Creator)
                .Include(i => i.Issue!)
                    .ThenInclude(issue => issue.IssueDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: Invoices/PrintPdf/5
        public async Task<IActionResult> PrintPdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Issue!)
                    .ThenInclude(issue => issue.Creator)
                .Include(i => i.Issue!)
                    .ThenInclude(issue => issue.IssueDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null) return NotFound();

            var document = QuestDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("HỆ THỐNG QUẢN LÝ KHO HÀNG").Bold().FontSize(16);
                        col.Item().Text($"HOÁ ĐƠN BÁN HÀNG ({invoice.InvoiceCode})").FontSize(14).SemiBold();
                        col.Item().Text($"Ngày lập: {invoice.CreatedDate:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Khách hàng: {invoice.CustomerName}");
                        col.Item().Text($"Phiếu xuất gốc: {invoice.Issue?.IssueCode} (Nhân viên: {invoice.Issue?.Creator?.Fullname})");
                        col.Item().LineHorizontal(1);
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                       {
                           columns.ConstantColumn(30);
                           columns.RelativeColumn(3);
                           columns.RelativeColumn(1);
                           columns.RelativeColumn(1);
                           columns.RelativeColumn(2);
                           columns.RelativeColumn(2);
                       });

                        table.Header(header =>
                       {
                           header.Cell().Text("#").Bold();
                           header.Cell().Text("Tên hàng").Bold();
                           header.Cell().Text("ĐVT").Bold();
                           header.Cell().Text("SL").Bold();
                           header.Cell().Text("Đơn giá").Bold();
                           header.Cell().Text("Thành tiền").Bold();
                       });

                        int stt = 1;
                        foreach (var item in invoice.Issue!.IssueDetails)
                        {
                            var price = item.Product?.Price ?? 0;
                            var lineTotal = price * item.Quantity;

                            table.Cell().Text(stt++.ToString());
                            table.Cell().Text(item.Product?.ProductName ?? "");
                            table.Cell().Text(item.Product?.Unit ?? "");
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text(price.ToString("N0") + " đ");
                            table.Cell().Text(lineTotal.ToString("N0") + " đ");
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1);
                        col.Item().AlignRight().Text($"TỔNG TIỀN THANH TOÁN: {invoice.TotalAmount:N0} VNĐ").Bold().FontSize(13);
                    });
                });
            });

            byte[] pdfData = document.GeneratePdf();
            return File(pdfData, "application/pdf", $"Invoice_{invoice.InvoiceCode}.pdf");
        }
    }
}

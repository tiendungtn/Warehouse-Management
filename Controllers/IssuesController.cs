using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;
using QuanLyKho.ViewModels;

namespace QuanLyKho.Controllers
{
    [Authorize]
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Issues
        public async Task<IActionResult> Index()
        {
            var issues = await _context.Issues
                .Include(i => i.Creator)
                .OrderByDescending(i => i.IssueDate)
                .AsNoTracking()
                .ToListAsync();
            return View(issues);
        }

        // GET: Issues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var issue = await _context.Issues
                .Include(i => i.Creator)
                .Include(i => i.Invoice)
                .Include(i => i.IssueDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (issue == null) return NotFound();

            return View(issue);
        }

        // GET: Issues/Create
        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.AsNoTracking().ToList();
            return View(new IssueCreateViewModel());
        }

        //POST: Issues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Create(IssueCreateViewModel model)
        {
            if (model.Items == null || !model.Items.Any(i => i.Quantity > 0))
            {
                ModelState.AddModelError("", "Phiếu xuất phải có ít nhất một mặt hàng với số lượng lớn hơn 0.");
            }
            if (ModelState.IsValid)
            {
                foreach (var item in model.Items.Where(i => i.Quantity > 0))
                {
                    var prod = await _context.Products.FindAsync(item.ProductId);
                    if (prod == null || prod.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Mặt hàng '{prod?.ProductName ?? "Chưa rõ"}' hiện chỉ còn tồn {prod?.StockQuantity ?? 0}, không đủ số lượng để xuất");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (String.IsNullOrEmpty(userIdString)) return Challenge();

                var issue = new Issue
                {
                    IssueCode = "PX-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Reason = model.Reason,
                    CreatedBy = int.Parse(userIdString),
                    IssueDate = DateTime.Now,
                    Status = "Pending"
                };

                foreach (var item in model.Items.Where(i => i.Quantity > 0))
                {
                    issue.IssueDetails.Add(new IssueDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }

                _context.Issues.Add(issue);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tạo phiếu xuất {issue.IssueCode} thành công (Chờ duyệt).";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Products = _context.Products.AsNoTracking().ToList();
            return View(model);
        }

        // POST: Issues/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(int id, string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                TempData["Error"] = "Vui lòng nhập tên khách hàng để lập hoá đơn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var issue = await _context.Issues
                    .Include(i => i.IssueDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (issue == null) return NotFound();

                if (issue.Status != "Pending")
                {
                    TempData["Error"] = "Phiếu xuất này đã được duyệt hoặc huỷ.";
                    return RedirectToAction(nameof(Index));
                }
                decimal totalAmount = 0;

                // Kiểm tra điều kiện tồn kho thực tế
                foreach (var detail in issue.IssueDetails)
                {
                    if (detail.Product == null || detail.Product.StockQuantity < detail.Quantity)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = $"Từ chối xuất kho: Mặt hàng '{detail.Product?.ProductName}' không đủ tồn kho để đáp ứng.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    detail.Product.StockQuantity -= detail.Quantity;
                    totalAmount += detail.Quantity * detail.Product.Price;
                }
                issue.Status = "Approved";

                var invoice = new Invoice
                {
                    InvoiceCode = "HD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    IssueId = issue.Id,
                    CustomerName = customerName.Trim(),
                    TotalAmount = totalAmount,
                    CreatedDate = DateTime.Now
                };

                _context.Invoices.Add(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Duyệt xuất kho thành công! Đã tự động lập hoá đơn {invoice.InvoiceCode}.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Lỗi xử lý duyệt xuất kho: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

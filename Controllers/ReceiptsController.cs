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
    public class ReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Receipts
        public async Task<IActionResult> Index()
        {
            var receipts = await _context.Receipts
                .Include(r => r.Creator)
                .OrderByDescending(r => r.ReceiptDate)
                .AsNoTracking()
                .ToListAsync();
            return View(receipts);
        }

        // GET: Receipts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipts
                .Include(r => r.Creator)
                .Include(r => r.ReceiptDetails)
                .ThenInclude(rd => rd.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // GET: Receipts/Create
        [HttpGet]
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.AsNoTracking().ToList();

            return View(new ReceiptCreateViewModel());
        }

        //POST: Receipts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Staff")]
        public async Task<IActionResult> Create(ReceiptCreateViewModel model)
        {

            if (model.Items == null || !model.Items.Any(i => i.Quantity > 0))
            {
                ModelState.AddModelError("", "Phiếu nhập phải có ít nhất một mặt hàng với số lượng lớn hơn 0.");
            }

            if (!ModelState.IsValid)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString)) return Challenge();

                var receipt = new Receipt
                {
                    ReceiptCode = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    SupplierName = model.SupplierName,
                    CreatedBy = int.Parse(userIdString),
                    ReceiptDate = DateTime.Now,
                    Status = "Pending"
                };

                foreach (var item in model.Items.Where(i => i.Quantity > 0))
                {
                    receipt.ReceiptDetails.Add(new ReceiptDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ImportPrice = item.ImportPrice
                    });
                }
                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tạo thành công phiếu nhập {receipt.ReceiptCode} (Chờ duyệt).";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Products = _context.Products.AsNoTracking().ToList();
            return View(model);
        }
    }
}

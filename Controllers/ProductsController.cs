using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách hàng hoá và tồn kho
        // GET: Products (Nhân viên kho có quyền xem danh sách tồn kho)
        [HttpGet]
        [Authorize(Roles = "Admin, Manager,Staff")]
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Tìm kiếm theo tên sản phẩm hoặc mã sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.ProductName.Contains(searchString) || p.ProductCode.Contains(searchString));
            }

            return View(await query.ToListAsync());
        }

        // 
        // GET: Products/Create
        [HttpGet]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "CategoryName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductCode,ProductName,CategoryId,Unit,Price,StockQuantity")] Product product)
        {
            if (await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode))
            {
                ModelState.AddModelError("ProductCode", "Mã sản phẩm đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit
        [HttpGet]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductCode,ProductName,CategoryId,Unit,Price,StockQuantity")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            bool isDuplicate = await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode && p.Id != product.Id);
            if (isDuplicate)
            {
                ModelState.AddModelError("ProductCode", "Mã sản phẩm đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.ProductCode = product.ProductCode.Trim().ToUpper(); // Chuyển đổi mã sản phẩm thành chữ hoa
                    product.ProductName = product.ProductName.Trim(); // Loại bỏ khoảng trắng thừa ở đầu và cuối tên sản phẩm
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Products.AnyAsync(e => e.Id == product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            bool hasReceiptHistory = await _context.ReceiptDetails.AnyAsync(rh => rh.ProductId == id);
            bool hasIssueHistory = await _context.IssueDetails.AnyAsync(ih => ih.ProductId == id);

            if (hasReceiptHistory || hasIssueHistory)
            {
                TempData["Error"] = $"Không thể xóa {product.ProductName} vì đã có lịch sử xuất/nhập kho. Hãy xem xét cập nhật số lượng hoặc huỷ kích hoạt!";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Xóa sản phẩm {product.ProductName} thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}

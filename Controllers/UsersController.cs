using Microsoft.AspNetCore.Authorization;
using QuanLyKho.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Models;
using System.Security.Claims;

namespace QuanLyKho.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tạo tài khoản '{user.Username}' thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string fullname, string role, string? newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Fullname = fullname;
            user.Role = role;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Cập nhật tài khoản '{user.Username}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id.ToString())
            {
                TempData["Error"] = "Bạn không thể tự do xoá tài khoản đang đăng nhập của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .Include(u => u.Receipts)
                .Include(u => u.Issues)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (user.Receipts.Any() || user.Issues.Any())
            {
                TempData["Error"] = $"Không thể xoá tài khoản '{user.Username}' vì đã có lịch lập phiếu trong hệ thống";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xoá người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
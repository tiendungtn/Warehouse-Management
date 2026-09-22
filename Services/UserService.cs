using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.DTOs;
using QuanLyKho.Models;

namespace QuanLyKho.Services;
public sealed class UserService
{
    private static readonly string[] ValidRoles =
    {
        "Admin",
        "Manager",
        "Staff"
    };

    private readonly ApplicationDbContext _context;

    public UserService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserDto>>
        GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .Select(x =>
                new UserDto(
                    x.Id,
                    x.Username,
                    x.Fullname,
                    x.Role,
                    x.CreatedAt
                ))
            .ToListAsync();
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request)
    {
        ValidateRole(request.Role);

        var username =
            request.Username.Trim();

        if (await _context.Users.AnyAsync(
                x => x.Username == username))
        {
            throw new InvalidOperationException(
                "Tên đăng nhập đã được sử dụng.");
        }

        if (string.IsNullOrWhiteSpace(
                request.Password))
        {
            throw new ArgumentException(
                "Mật khẩu không được để trống.");
        }

        var user = new User
        {
            Username = username,

            Password =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password),

            Fullname =
                request.Fullname.Trim(),

            Role =
                request.Role.Trim(),

            CreatedAt =
                DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new UserDto(
            user.Id,
            user.Username,
            user.Fullname,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task<UserDto> UpdateAsync(
        int id,
        UpdateUserRequest request)
    {
        ValidateRole(request.Role);

        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (user == null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy người dùng.");
        }

        user.Fullname =
            request.Fullname.Trim();

        user.Role =
            request.Role.Trim();

        if (!string.IsNullOrWhiteSpace(
                request.NewPassword))
        {
            user.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    request.NewPassword);
        }

        await _context.SaveChangesAsync();

        return new UserDto(
            user.Id,
            user.Username,
            user.Fullname,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task DeleteAsync(
        int id,
        int currentUserId)
    {
        if (id == currentUserId)
        {
            throw new InvalidOperationException(
                "Bạn không thể tự xóa " +
                "tài khoản đang đăng nhập.");
        }

        var user =
            await _context.Users
                .Include(x => x.Receipts)
                .Include(x => x.Issues)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (user == null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy người dùng.");
        }

        if (user.Receipts.Count > 0 ||
            user.Issues.Count > 0)
        {
            throw new InvalidOperationException(
                $"Không thể xóa tài khoản " +
                $"'{user.Username}' vì đã có " +
                "lịch sử lập phiếu.");
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    private static void ValidateRole(
        string role)
    {
        if (!ValidRoles.Contains(
                role.Trim(),
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Role phải là Admin, Manager hoặc Staff.");
        }
    }
}

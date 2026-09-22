using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyKho.Data;
using QuanLyKho.DTOs;

namespace QuanLyKho.Services;

public sealed class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        var username = request.Username.Trim();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Username == username);

        if (user is null ||
            !BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.Password))
        {
            throw new UnauthorizedAccessException(
                "Tài khoản hoặc mật khẩu không chính xác.");
        }

        var expiresAt = DateTime.UtcNow.Add(
            request.RememberMe
                ? TimeSpan.FromDays(30)
                : TimeSpan.FromHours(8));

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.GivenName,
                user.Fullname),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAt,
            new AuthUserDto(
                user.Id,
                user.Username,
                user.Fullname,
                user.Role));
    }

    public async Task<AuthUserDto> MeAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy người dùng.");

        return new AuthUserDto(
            user.Id,
            user.Username,
            user.Fullname,
            user.Role);
    }
}
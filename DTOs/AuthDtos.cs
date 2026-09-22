using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.DTOs;

public sealed class LoginRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}

public sealed record AuthUserDto(
    int Id,
    string Username,
    string Fullname,
    string Role);

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    AuthUserDto User);
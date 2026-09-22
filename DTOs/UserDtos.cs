using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.DTOs;

public sealed record UserDto(
    int Id,
    string Username,
    string Fullname,
    string Role,
    DateTime CreatedAt);

public sealed class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "";

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Fullname { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Staff";
}

public sealed class UpdateUserRequest
{
    [Required]
    [MaxLength(200)]
    public string Fullname { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Staff";

    public string? NewPassword { get; set; }
}
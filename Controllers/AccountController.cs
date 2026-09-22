using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyKho.DTOs;
using QuanLyKho.Services;
using System.Security.Claims;

namespace QuanLyKho.Controllers;
[ApiController]
[Route("api/auth")]
public sealed class AccountController : ControllerBase
{
    private readonly AuthService _service;

    public AccountController(AuthService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        return Ok(
            await _service.LoginAsync(request));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        return Ok(
            await _service.MeAsync(userId));
    }
}

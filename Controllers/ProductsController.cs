using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyKho.DTOs;
using QuanLyKho.Services;

namespace QuanLyKho.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductService _service;

    public ProductsController(
        ProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search)
    {
        return Ok(
            await _service.GetAllAsync(search));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(
            await _service.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(
        ProductRequest request)
    {
        var result =
            await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(
        int id,
        ProductRequest request)
    {
        return Ok(
            await _service.UpdateAsync(
                id,
                request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int id)
    {
        await _service.DeleteAsync(id);

        return NoContent();
    }
}

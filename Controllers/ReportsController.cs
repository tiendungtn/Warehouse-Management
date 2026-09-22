using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyKho.Services;

namespace QuanLyKho.Controllers;
[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController
    : ControllerBase
{
    private readonly ReportService _service;

    public ReportsController(
        ReportService service)
    {
        _service = service;
    }

    [HttpGet("stock")]
    [Authorize(Roles =
        "Admin,Manager,Staff")]
    public async Task<IActionResult> Stock(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? stockFilter)
    {
        return Ok(
            await _service.GetStockReportAsync(
                search,
                categoryId,
                stockFilter));
    }

    [HttpGet("stock/export")]
    [Authorize(Roles =
        "Admin,Manager")]
    public async Task<IActionResult>
        ExportStock()
    {
        var result =
            await _service
                .ExportStockCsvAsync();

        return File(
            result.Content,
            "text/csv; charset=utf-8",
            result.FileName);
    }

    [HttpGet("revenue")]
    [Authorize(Roles =
        "Admin,Manager")]
    public async Task<IActionResult> Revenue(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        return Ok(
            await _service
                .GetRevenueReportAsync(
                    fromDate,
                    toDate));
    }
}
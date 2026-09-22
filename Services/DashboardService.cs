using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.DTOs;

namespace QuanLyKho.Services;
public sealed class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto>
        GetAsync()
    {
        var totalProducts =
            await _context.Products
                .CountAsync();

        var totalCategories =
            await _context.Categories
                .CountAsync();

        var totalUsers =
            await _context.Users
                .CountAsync();

        var pendingReceipts =
            await _context.Receipts
                .CountAsync(
                    x => x.Status == "Pending");

        var pendingIssues =
            await _context.Issues
                .CountAsync(
                    x => x.Status == "Pending");

        var inventoryValue =
            await _context.Products
                .Select(
                    x =>
                        (decimal)x.StockQuantity *
                        x.Price)
                .SumAsync();

        var revenue =
            await _context.Invoices
                .Select(x => x.TotalAmount)
                .SumAsync();

        return new DashboardDto(
            totalProducts,
            totalCategories,
            totalUsers,
            pendingReceipts,
            pendingIssues,
            inventoryValue,
            revenue
        );
    }
}

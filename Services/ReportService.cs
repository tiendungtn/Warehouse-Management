using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.DTOs;
using System.Text;

namespace QuanLyKho.Services;
public sealed class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockReportDto>
        GetStockReportAsync(
            string? search,
            int? categoryId,
            string? stockFilter)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(x =>
                x.ProductCode.Contains(keyword) ||
                x.ProductName.Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(
                x => x.CategoryId ==
                     categoryId.Value);
        }

        switch (
            stockFilter?
                .Trim()
                .ToLowerInvariant())
        {
            case "outofstock":

                query = query.Where(
                    x => x.StockQuantity == 0);

                break;

            case "lowstock":

                query = query.Where(
                    x =>
                        x.StockQuantity > 0 &&
                        x.StockQuantity <= 5);

                break;

            case "instock":

                query = query.Where(
                    x =>
                        x.StockQuantity > 5);

                break;
        }

        var products = await query
            .OrderBy(x => x.StockQuantity)
            .ThenBy(x => x.ProductName)
            .Select(x =>
                new StockReportItemDto(
                    x.Id,
                    x.ProductCode,
                    x.ProductName,
                    x.Category != null
                        ? x.Category.CategoryName
                        : string.Empty,
                    x.Unit,
                    x.Price,
                    x.StockQuantity,
                    x.StockQuantity *
                        x.Price,
                    x.StockQuantity == 0
                        ? "OutOfStock"
                        : x.StockQuantity <= 5
                            ? "LowStock"
                            : "InStock"
                ))
            .ToListAsync();

        return new StockReportDto(
            products.Count,

            products.Sum(
                x => x.StockQuantity),

            products.Sum(
                x => x.TotalValuation),

            products.Count(
                x => x.StockQuantity == 0),

            products.Count(
                x =>
                    x.StockQuantity > 0 &&
                    x.StockQuantity <= 5),

            products
        );
    }

    public async Task<RevenueReportDto>
        GetRevenueReportAsync(
            DateTime? fromDate,
            DateTime? toDate)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(x => x.Issue)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var fromUtc =
                DateTime.SpecifyKind(
                    fromDate.Value.Date,
                    DateTimeKind.Utc);

            query = query.Where(
                x => x.CreatedDate >=
                     fromUtc);
        }

        if (toDate.HasValue)
        {
            var endDate =
                toDate.Value.Date
                    .AddDays(1)
                    .AddTicks(-1);

            var endUtc =
                DateTime.SpecifyKind(
                    endDate,
                    DateTimeKind.Utc);

            query = query.Where(
                x => x.CreatedDate <=
                     endUtc);
        }

        var invoices =
            await query
                .OrderByDescending(
                    x => x.CreatedDate)
                .Select(x =>
                    new InvoiceListDto(
                        x.Id,
                        x.InvoiceCode,
                        x.IssueId,
                        x.Issue!.IssueCode,
                        x.CustomerName,
                        x.TotalAmount,
                        x.CreatedDate
                    ))
                .ToListAsync();

        return new RevenueReportDto(
            fromDate,
            toDate,
            invoices.Sum(
                x => x.TotalAmount),
            invoices
        );
    }

    public async Task<(
        byte[] Content,
        string FileName
    )> ExportStockCsvAsync()
    {
        var report =
            await GetStockReportAsync(
                null,
                null,
                null);

        var builder =
            new StringBuilder();

        builder.AppendLine(
            "Mã hàng,Tên hàng,Danh mục," +
            "ĐVT,Đơn giá,Tồn kho," +
            "Tổng giá trị tồn");

        foreach (
            var item in report.Products)
        {
            builder.AppendLine(
                $"{Escape(item.ProductCode)}," +
                $"{Escape(item.ProductName)}," +
                $"{Escape(item.CategoryName)}," +
                $"{Escape(item.Unit)}," +
                $"{item.Price}," +
                $"{item.StockQuantity}," +
                $"{item.TotalValuation}"
            );
        }

        var preamble =
            Encoding.UTF8.GetPreamble();

        var content =
            Encoding.UTF8.GetBytes(
                builder.ToString());

        var result = new byte[
            preamble.Length +
            content.Length
        ];

        Buffer.BlockCopy(
            preamble,
            0,
            result,
            0,
            preamble.Length);

        Buffer.BlockCopy(
            content,
            0,
            result,
            preamble.Length,
            content.Length);

        return (
            result,
            $"BaoCaoTonKho_" +
            $"{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        );
    }

    private static string Escape(
        string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        return "\"" +
            value.Replace("\"", "\"\"") +
            "\"";
    }
}

using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.DTOs;
using QuanLyKho.Models;

namespace QuanLyKho.Services;
public sealed class ReceiptService
{
    private readonly ApplicationDbContext _context;

    public ReceiptService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReceiptListDto>> GetAllAsync()
    {
        return await _context.Receipts
            .AsNoTracking()
            .Include(x => x.Creator)
            .Include(x => x.ReceiptDetails)
            .OrderByDescending(x => x.ReceiptDate)
            .Select(x => new ReceiptListDto(
                x.Id,
                x.ReceiptCode,
                x.SupplierName,
                x.CreatedBy,
                x.Creator != null
                    ? x.Creator.Fullname
                    : string.Empty,
                x.ReceiptDate,
                x.Status,
                x.ReceiptDetails.Sum(d =>
                    d.Quantity * d.ImportPrice)
            ))
            .ToListAsync();
    }

    public async Task<ReceiptDto> GetByIdAsync(int id)
    {
        var receipt = await _context.Receipts
            .AsNoTracking()
            .Include(x => x.Creator)
            .Include(x => x.ReceiptDetails)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (receipt == null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy phiếu nhập.");
        }

        var items = receipt.ReceiptDetails
            .Select(x => new ReceiptDetailDto(
                x.ProductId,
                x.Product?.ProductCode ?? string.Empty,
                x.Product?.ProductName ?? string.Empty,
                x.Product?.Unit ?? string.Empty,
                x.Quantity,
                x.ImportPrice,
                x.Quantity * x.ImportPrice
            ))
            .ToList();

        return new ReceiptDto(
            receipt.Id,
            receipt.ReceiptCode,
            receipt.SupplierName,
            receipt.CreatedBy,
            receipt.Creator?.Fullname ?? string.Empty,
            receipt.ReceiptDate,
            receipt.Status,
            items.Sum(x => x.TotalAmount),
            items
        );
    }

    public async Task<ReceiptDto> CreateAsync(
        ReceiptRequest request,
        int userId)
    {
        if (string.IsNullOrWhiteSpace(
                request.SupplierName))
        {
            throw new ArgumentException(
                "Vui lòng nhập tên nhà cung cấp.");
        }

        var validItems = request.Items
            .Where(x => x.Quantity > 0)
            .ToList();

        if (validItems.Count == 0)
        {
            throw new InvalidOperationException(
                "Phiếu nhập phải có ít nhất một mặt hàng.");
        }

        var productIds = validItems
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        foreach (var item in validItems)
        {
            if (!products.ContainsKey(item.ProductId))
            {
                throw new InvalidOperationException(
                    $"Sản phẩm ID {item.ProductId} không tồn tại.");
            }
        }

        var receipt = new Receipt
        {
            ReceiptCode = await GenerateReceiptCodeAsync(),
            SupplierName = request.SupplierName.Trim(),
            CreatedBy = userId,
            ReceiptDate =
                request.ReceiptDate?.ToUniversalTime()
                ?? DateTime.UtcNow,
            Status = "Pending"
        };

        foreach (var item in validItems)
        {
            receipt.ReceiptDetails.Add(
                new ReceiptDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ImportPrice = item.ImportPrice
                });
        }

        _context.Receipts.Add(receipt);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(receipt.Id);
    }

    public async Task ApproveAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var receipt = await _context.Receipts
                .Include(x => x.ReceiptDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy phiếu nhập.");
            }

            if (receipt.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "Phiếu nhập này đã được xử lý trước đó.");
            }

            foreach (var detail in receipt.ReceiptDetails)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        x => x.Id == detail.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm ID {detail.ProductId} " +
                        "không tồn tại.");
                }

                product.StockQuantity += detail.Quantity;
            }

            receipt.Status = "Approved";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<string> GenerateReceiptCodeAsync()
    {
        var prefix =
            $"PN-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var code = prefix;
        var suffix = 1;

        while (await _context.Receipts
            .AnyAsync(x => x.ReceiptCode == code))
        {
            code = $"{prefix}-{suffix}";
            suffix++;
        }

        return code;
    }
}

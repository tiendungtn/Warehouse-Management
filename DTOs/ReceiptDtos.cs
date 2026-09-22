using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.DTOs;

public sealed record ReceiptDetailDto(
    int ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    decimal ImportPrice,
    decimal TotalAmount);

public sealed record ReceiptListDto(
    int Id,
    string ReceiptCode,
    string SupplierName,
    int CreatedBy,
    string CreatorName,
    DateTime ReceiptDate,
    string Status,
    decimal TotalAmount);

public sealed record ReceiptDto(
    int Id,
    string ReceiptCode,
    string SupplierName,
    int CreatedBy,
    string CreatorName,
    DateTime ReceiptDate,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<ReceiptDetailDto> Items);

public sealed class ReceiptRequest
{
    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = "";

    public DateTime? ReceiptDate { get; set; }

    [MinLength(1)]
    public List<ReceiptItemRequest> Items { get; set; } = [];
}

public sealed class ReceiptItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    [Range(0, 10000000000)]
    public decimal ImportPrice { get; set; }
}

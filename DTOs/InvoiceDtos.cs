namespace QuanLyKho.DTOs;

public sealed record InvoiceListDto(
    int Id,
    string InvoiceCode,
    int IssueId,
    string IssueCode,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedDate);

public sealed record InvoiceDto(
    int Id,
    string InvoiceCode,
    int IssueId,
    string IssueCode,
    string CustomerName,
    string CreatorName,
    decimal TotalAmount,
    DateTime CreatedDate,
    IReadOnlyList<InvoiceItemDto> Items);

public sealed record InvoiceItemDto(
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount);
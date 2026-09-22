namespace QuanLyKho.DTOs;

public sealed record StockReportItemDto(
    int ProductId,
    string ProductCode,
    string ProductName,
    string CategoryName,
    string Unit,
    decimal Price,
    int StockQuantity,
    decimal TotalValuation,
    string Status);

public sealed record StockReportDto(
    int TotalProductCount,
    int TotalStockQuantity,
    decimal TotalStockValuation,
    int OutOfStockCount,
    int LowStockCount,
    IReadOnlyList<StockReportItemDto> Products);

public sealed record RevenueReportDto(
    DateTime? FromDate,
    DateTime? ToDate,
    decimal TotalRevenue,
    IReadOnlyList<InvoiceListDto> Invoices);

public sealed record DashboardDto(
    int TotalProducts,
    int TotalCategories,
    int TotalUsers,
    int PendingReceipts,
    int PendingIssues,
    decimal InventoryValue,
    decimal Revenue);
using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.ViewModels
{
    public class InventoryReportViewModel
    {
        [Display(Name = "Từ khoá tìm kiếm")]
        public string? SearchString { get; set; }

        [Display(Name = "Danh mục")]
        public int? SelectedCategoryId { get; set; }

        [Display(Name = "Trạng thái tồn")]
        public string? StockFilter { get; set; } // "All", "OutOfStock", "LowStock", "InStock"

        public List<InventoryItemViewModel> Products { get; set; } = new List<InventoryItemViewModel>();

        [Display(Name = "Tổng số mặt hàng")]
        public int TotalProductCount => Products.Count;

        [Display(Name = "Tổng số lượng tồn kho")]
        public int TotalStockQuantity => Products.Sum(p => p.StockQuantity);

        [Display(Name = "Tổng giá trị kho")]
        public decimal TotalStockValuation => Products.Sum(p => p.TotalValuation);

        [Display(Name = "Hàng đã hết (Tồn = 0)")]
        public int OutOfStockCount => Products.Count(p => p.StockQuantity == 0);

        [Display(Name = "Hàng sắp hết (Tồn <= 5)")]
        public int LowStockCount => Products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 5);


    }

    public class InventoryItemViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Mã hàng")]
        public string ProductCode { get; set; } = string.Empty;

        [Display(Name = "Tên hàng")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Danh mục")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Đơn vị")]
        public string Unit { get; set; } = string.Empty;

        [Display(Name = "Đơn giá")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng tồn")]
        public int StockQuantity { get; set; }

        [Display(Name = "Tổng giá trị")]
        public decimal TotalValuation => StockQuantity * Price;

        public string StatusBadgeClass => StockQuantity switch
        {
            0 => "badge bg-danger", // Hết hàng: màu đỏ
            <= 5 => "badge bg-warning text-dark", // Sắp hết hàng: màu vàng
            _ => "badge bg-success" // Còn hàng: màu xanh
        };

        public string StatusText => StockQuantity switch
        {
            0 => "Hết hàng",
            <= 5 => "Sắp hết hàng",
            _ => "Còn hàng"
        };
    }
}

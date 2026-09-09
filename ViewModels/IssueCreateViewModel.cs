using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.ViewModels
{
    public class IssueCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do xuất kho.")]
        [MaxLength(255, ErrorMessage = "Lý do xuất kho không được vượt quá 255 ký tự.")]
        [Display(Name = "Lý do xuất kho")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Tên khách hàng / Đơn vị nhận")]
        [StringLength(150, ErrorMessage = "Tên khách hàng không được vượt quá 150 ký tự")]
        public string? CustomerName { get; set; }

        [Display(Name = "Ngày lập phiếu")]
        [DataType(DataType.DateTime)]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public List<IssueItemViewModel> Items { get; set; } = new List<IssueItemViewModel>();

        [Display(Name = "Tổng số lượng phiếu")]
        public int TotalQuantity => Items?.Sum(x => x.Quantity) ?? 0;
    }

    public class IssueItemViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm được chọn không hợp lệ.")]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }

        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }

        [Display(Name = "Tồn khả dụng")]
        public int AvailableStock { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng nhập số lượng xuất.")]
        [Range(1, 1000000, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 1.")]
        [Display(Name = "Số lượng xuất")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Đơn giá niêm yết")]
        public decimal UnitPrice { get; set; } = 0;

        [Display(Name = "Thành tiền dự kiến")]
        public decimal TotalLineAmount => Quantity * UnitPrice;
    }

    public class IssueItemInputModel : IssueItemViewModel { }
}

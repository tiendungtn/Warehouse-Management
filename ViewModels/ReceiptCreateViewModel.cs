using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.ViewModels
{
    public class ReceiptCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp.")]
        [MaxLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên nhà cung cấp")]
        public string SupplierName { get; set; } = new string(string.Empty);

        [Display(Name = "Ngày lập phiếu")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        public List<ReceiptItemViewModel> Items { get; set; } = new List<ReceiptItemViewModel>();

        [Display(Name = "Tổng giá trị phiếu")]
        public decimal TotalReceiptAmount => Items?.Sum(x => x.TotalLineAmount) ?? 0;
    }

    public class ReceiptItemViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm được chọn không hợp lệ.")]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }

        public string? ProductName { get; set; }
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, 1000000, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng nhập giá nhập.")]
        [Range(0, 10000000000, ErrorMessage = "Đơn giá nhập phải lớn hơn 0.")]
        [Display(Name = "Giá nhập (VNĐ)")]
        [DataType(DataType.Currency)]
        public decimal ImportPrice { get; set; }

        [Display(Name = "Thành tiền")]
        public decimal TotalLineAmount => Quantity * ImportPrice;
    }

    public class ReceiptItemInputModel : ReceiptItemViewModel { }

    public class ReceiptDetailViewModel : ReceiptItemViewModel { }

    public class ReceiptDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã phiếu nhập")]
        public string ReceiptCode { get; set; } = new string(string.Empty);

        [Display(Name = "Nhà cung cấp")]
        public string SupplierName { get; set; } = new string(string.Empty);

        [Display(Name = "Người lập")]
        public string CreatorName { get; set; } = new string(string.Empty);

        [Display(Name = "Ngày lập")]
        public DateTime ReceiptDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Đang chờ";

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        public List<ReceiptDetailViewModel> Items { get; set; } = new List<ReceiptDetailViewModel>();

        [Display(Name = "Tổng tiền")]
        public decimal TotalReceiptAmount => Items?.Sum(x => x.TotalLineAmount) ?? 0;
    }
}

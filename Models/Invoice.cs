using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace QuanLyKho.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Mã hóa đơn")]
        public string InvoiceCode { get; set; } = string.Empty;

        [Required]
        public int IssueId { get; set; }

        [ForeignKey(nameof(IssueId))]
        public virtual Issue? Issue { get; set; }

        [Required, MaxLength(150)]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng số tiền")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

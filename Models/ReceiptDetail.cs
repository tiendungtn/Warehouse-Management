using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKho.Models
{
    public class ReceiptDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReceiptId { get; set; }

        [ForeignKey(nameof(ReceiptDetail.Id))]
        public virtual Receipt? Receipt { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        [Required]
        [Display(Name = "Số lượng nhập")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal ImportPrice { get; set; }  
    }
}

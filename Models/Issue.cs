using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models
{
    public class Issue
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Display(Name = "Mã phiếu xuất")]
        public string IssueCode { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Lý do xuất")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public int CreatedBy { get; set; }

        public virtual User? Creator { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(30)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chờ duyệt";

        public virtual ICollection<IssueDetail> IssueDetails { get; set; } = new List<IssueDetail>();
        public virtual Invoice? Invoice { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneMVC.Models
{
    public enum RequestStatus
    {
        Pending,      // Beklemede
        Approved,     // Onaylandı
        Rejected,     // Reddedildi
        Cancelled     // İptal Edildi
    }

    public class LoanRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }

        [ForeignKey("RequestedByUserId")]
        public User? RequestedByUser { get; set; }

        [Required]
        public int RequestedForMemberId { get; set; }

        [ForeignKey("RequestedForMemberId")]
        public Member? RequestedForMember { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public RequestStatus RequestStatus { get; set; } = RequestStatus.Pending;

        [Required]
        [Display(Name = "İstenen İade Tarihi")]
        public DateTime RequestedDueDate { get; set; } = DateTime.Now.AddDays(15);

        [StringLength(500, ErrorMessage = "Admin notu en fazla 500 karakter olabilir")]
        [Display(Name = "Admin Notu")]
        public string? AdminNotes { get; set; }

        public int? ProcessedByUserId { get; set; }

        [ForeignKey("ProcessedByUserId")]
        public User? ProcessedByUser { get; set; }

        public DateTime? ProcessedDate { get; set; }
    }
}

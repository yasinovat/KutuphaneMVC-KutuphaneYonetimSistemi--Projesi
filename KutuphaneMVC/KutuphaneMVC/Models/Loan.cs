using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KutuphaneMVC.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Display(Name = "Kitap")]
        [ForeignKey("Book")]
        [Required(ErrorMessage = "Kitap seçimi zorunludur.")]
        public int BookId { get; set; }

        
        [ValidateNever]
        public Book? Book { get; set; }

        [Display(Name = "Üye")]
        [ForeignKey("Member")]
        [Required(ErrorMessage = "Üye seçimi zorunludur.")]
        public int MemberId { get; set; }

        [ValidateNever]
        public Member? Member { get; set; }

        [Display(Name = "Alış Tarihi")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Alış tarihi zorunludur.")]
        public DateTime BorrowDate { get; set; } = DateTime.Today;

        [Display(Name = "İade Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Teslim Edilmesi Gereken Tarih")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Teslim edilmesi gereken tarih zorunludur.")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
    }
}



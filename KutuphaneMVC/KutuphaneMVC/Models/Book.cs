using System.ComponentModel.DataAnnotations;

namespace KutuphaneMVC.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Display(Name = "Kitap Adı")]
        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "Yazar")]
        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        [StringLength(100)]
        public string Author { get; set; }

        [Display(Name = "Tür")]
        [StringLength(50)]
        public string? Genre { get; set; }

        [Display(Name = "Stok Miktarı")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
        public int Stock { get; set; }

        // Yeni eklenen alan — Google Books'tan gelen kapak görselini tutar
        [Display(Name = "Kapak Görseli")]
        public string? ImageUrl { get; set; }
    }
}



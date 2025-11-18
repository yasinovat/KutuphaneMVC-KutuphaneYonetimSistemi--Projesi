using System.ComponentModel.DataAnnotations;

namespace KutuphaneMVC.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Üye adı zorunludur.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Display(Name = "Telefon Numarası")]
        [StringLength(11, ErrorMessage = "Telefon numarası en fazla 11 haneli olmalıdır.")]
        public string? Phone { get; set; }

        [Display(Name = "E-posta Adresi")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }
    }
}

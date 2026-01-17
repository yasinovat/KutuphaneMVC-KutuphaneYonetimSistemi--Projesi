using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneMVC.Models
{
    public enum UserRole
    {
        Admin,
        User
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [StringLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        // Kullanıcının bağlı olduğu Member kaydı (nullable - adminler için olmayabilir)
        public int? MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        // Navigation property - bu kullanıcının oluşturduğu loan requestler
        public ICollection<LoanRequest>? RequestedLoans { get; set; }

        // Navigation property - bu kullanıcının işleme aldığı loan requestler (admin için)
        public ICollection<LoanRequest>? ProcessedLoans { get; set; }
    }
}

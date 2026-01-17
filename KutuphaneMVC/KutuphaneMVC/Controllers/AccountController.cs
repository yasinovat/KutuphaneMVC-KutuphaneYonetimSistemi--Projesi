using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string email, string fullName, string? phone)
        {
            // Şifre kontrolü
            if (password != confirmPassword)
            {
                ViewBag.Error = "Şifreler eşleşmiyor!";
                return View();
            }

            // Şifre uzunluk kontrolü
            if (password.Length < 8)
            {
                ViewBag.Error = "Şifre en az 8 karakter olmalıdır!";
                return View();
            }

            // Kullanıcı adı kontrolü
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                return View();
            }

            // Email kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Bu email adresi zaten kullanılıyor!";
                return View();
            }

            // Transaction ile birlikte Member ve User oluştur
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Önce Member kaydı oluştur
                var member = new Member
                {
                    FullName = fullName,
                    Email = email,
                    Phone = phone
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                // Sonra User kaydı oluştur ve Member'a bağla
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    Role = UserRole.User,
                    MemberId = member.MemberId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ViewBag.Error = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                return View();
            }
        }
    }
}

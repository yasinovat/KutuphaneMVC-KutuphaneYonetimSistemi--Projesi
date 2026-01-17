using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly LibraryContext _context;

        public LoginController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            // Veritabanından kullanıcıyı bul
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Oturum değişkenlerini ayarla
                HttpContext.Session.SetInt32("userId", user.UserId);
                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("role", user.Role.ToString());
                
                // LastLoginDate güncelle
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync();

                // Role göre yönlendir
                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "UserDashboard");
                }
            }
            else
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
        }

        // Çıkış işlemi
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}

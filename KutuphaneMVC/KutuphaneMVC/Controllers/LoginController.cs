using Microsoft.AspNetCore.Mvc;

namespace KutuphaneMVC.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Basit doğrulama
            if (username == "admin" && password == "admin123")
            {
                // Oturum değişkeni ayarla
                HttpContext.Session.SetString("username", username);

                // Ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
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

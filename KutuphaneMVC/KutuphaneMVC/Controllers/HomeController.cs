using KutuphaneMVC.Filters;
using KutuphaneMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LibraryContext _context;

        //Logger + Veritabanı bağlantısı eklendi
        public HomeController(ILogger<HomeController> logger, LibraryContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                // Toplam kitap sayısı = Mevcut stok + Ödünç verilen kitaplar
                var totalStock = _context.Books.Sum(b => b.Stock);
                var activeLoanCount = _context.Loans.Count(l => l.ReturnDate == null);
                ViewBag.KitapSayisi = totalStock + activeLoanCount;

                // Toplam üye sayısı
                ViewBag.UyeSayisi = _context.Members.Count();

                // Henüz iade edilmemiş kitap sayısı
                ViewBag.OduncSayisi = activeLoanCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ana sayfa istatistikleri alınırken hata oluştu.");
                ViewBag.KitapSayisi = 0;
                ViewBag.UyeSayisi = 0;
                ViewBag.OduncSayisi = 0;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

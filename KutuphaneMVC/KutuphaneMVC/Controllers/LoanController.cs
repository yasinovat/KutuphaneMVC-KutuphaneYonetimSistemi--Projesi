using KutuphaneMVC.Filters;
using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KutuphaneMVC.Controllers
{
    [AuthFilter]
    public class LoanController : Controller
    {
        private readonly LibraryContext _context;

        public LoanController(LibraryContext context)
        {
            _context = context;
        }

        // 1. Ödünçteki kitapları listele
        public IActionResult Index()
        {
            var oduncler = _context.Loans
                .Include(o => o.Book)
                .Include(o => o.Member)
                .ToList();

            return View(oduncler);
        }

        // 2. Ödünç verme sayfası (GET)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Books = new SelectList(_context.Books.Where(b => b.Stock > 0).ToList(), "BookId", "Title");
            ViewBag.Members = new SelectList(_context.Members.ToList(), "MemberId", "FullName");

            // Varsayılan tarih değerleri
            var model = new Loan
            {
                BorrowDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15)
            };
            return View(model);
        }

        // 3. Ödünç verme işlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Loan loan)
        {
            // Boş tarih gelirse bugünü atayalım
            if (loan.BorrowDate == default) loan.BorrowDate = DateTime.Today;
            if (loan.DueDate == default) loan.DueDate = DateTime.Today.AddDays(15);

            // Validasyonlar
            if (loan.BookId == 0)
                ModelState.AddModelError("BookId", "Lütfen bir kitap seçiniz.");

            if (loan.MemberId == 0)
                ModelState.AddModelError("MemberId", "Lütfen bir üye seçiniz.");

            if (loan.DueDate < loan.BorrowDate)
                ModelState.AddModelError("DueDate", "Teslim tarihi alış tarihinden önce olamaz.");

            // Kayıt işlemi
            if (ModelState.IsValid)
            {
                var kitap = _context.Books.FirstOrDefault(b => b.BookId == loan.BookId);
                if (kitap is null)
                {
                    ModelState.AddModelError("BookId", "Seçilen kitap bulunamadı.");
                }
                else if (kitap.Stock <= 0)
                {
                    ModelState.AddModelError("BookId", "Seçilen kitap stokta yok.");
                }
                else
                {
                    try
                    {
                        kitap.Stock--;                 // stok azalt
                        _context.Loans.Add(loan);      // ödünç kaydı oluştur
                        _context.SaveChanges();        // veritabanına yaz
                        TempData["Success"] = "Kitap ödünç işlemi başarıyla kaydedildi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, $"Kayıt sırasında hata oluştu: {ex.Message}");
                    }
                }
            }

            // Hata varsa dropdown'ları yeniden yükle
            ViewBag.Books = new SelectList(_context.Books.Where(b => b.Stock > 0).ToList(), "BookId", "Title", loan.BookId);
            ViewBag.Members = new SelectList(_context.Members.ToList(), "MemberId", "FullName", loan.MemberId);

            return View(loan);
        }

        // 4. Kitap iade et
        public IActionResult ReturnBook(int id)
        {
            var odunc = _context.Loans
                .Include(o => o.Book)
                .FirstOrDefault(o => o.LoanId == id);

            if (odunc == null)
                return NotFound();

            if (odunc.ReturnDate == null)
            {
                odunc.ReturnDate = DateTime.Now;
                odunc.Book.Stock++;
                _context.SaveChanges();
                TempData["Success"] = "Kitap başarıyla iade edildi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}



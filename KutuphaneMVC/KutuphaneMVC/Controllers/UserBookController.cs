using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using KutuphaneMVC.Filters;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("User")]
    public class UserBookController : Controller
    {
        private readonly LibraryContext _context;

        public UserBookController(LibraryContext context)
        {
            _context = context;
        }

        // GET: UserBook/Index
        public async Task<IActionResult> Index(string? searchTitle, string? searchAuthor, string? searchGenre)
        {
            var query = _context.Books.AsQueryable();

            // Arama filtreleri
            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(b => b.Title.Contains(searchTitle));
                ViewBag.SearchTitle = searchTitle;
            }

            if (!string.IsNullOrEmpty(searchAuthor))
            {
                query = query.Where(b => b.Author.Contains(searchAuthor));
                ViewBag.SearchAuthor = searchAuthor;
            }

            if (!string.IsNullOrEmpty(searchGenre))
            {
                query = query.Where(b => b.Genre != null && b.Genre.Contains(searchGenre));
                ViewBag.SearchGenre = searchGenre;
            }

            var books = await query.OrderBy(b => b.Title).ToListAsync();

            return View(books);
        }

        // POST: UserBook/RequestLoan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLoan(int bookId, int requestedDays = 15)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.MemberId == null)
            {
                TempData["Error"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["Error"] = "Kitap bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Stok kontrolü
            if (book.Stock <= 0)
            {
                TempData["Error"] = "Bu kitap şu anda stokta yok.";
                return RedirectToAction(nameof(Index));
            }

            // Kullanıcının aynı kitap için bekleyen bir isteği var mı kontrol et
            var existingRequest = await _context.LoanRequests
                .AnyAsync(lr => lr.BookId == bookId && 
                               lr.RequestedByUserId == userId && 
                               lr.RequestStatus == RequestStatus.Pending);

            if (existingRequest)
            {
                TempData["Error"] = "Bu kitap için zaten bekleyen bir isteğiniz var.";
                return RedirectToAction(nameof(Index));
            }

            // Ödünç isteği oluştur
            var loanRequest = new LoanRequest
            {
                BookId = bookId,
                RequestedByUserId = user.UserId,
                RequestedForMemberId = user.MemberId.Value,
                RequestDate = DateTime.Now,
                RequestStatus = RequestStatus.Pending,
                RequestedDueDate = DateTime.Now.AddDays(requestedDays)
            };

            _context.LoanRequests.Add(loanRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{book.Title}\" kitabı için ödünç isteği başarıyla oluşturuldu. Admin onayı bekleniyor.";
            return RedirectToAction(nameof(Index));
        }
    }
}

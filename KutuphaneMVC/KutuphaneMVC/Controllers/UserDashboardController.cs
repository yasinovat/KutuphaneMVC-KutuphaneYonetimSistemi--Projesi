using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using KutuphaneMVC.Filters;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("User")]
    public class UserDashboardController : Controller
    {
        private readonly LibraryContext _context;

        public UserDashboardController(LibraryContext context)
        {
            _context = context;
        }

        // GET: UserDashboard/Index
        public async Task<IActionResult> Index()
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
                ViewBag.Error = "Kullanıcı bilgileri bulunamadı.";
                return View();
            }

            // İstatistikler
            var totalStock = await _context.Books.SumAsync(b => b.Stock);
            var activeLoanCount = await _context.Loans.Where(l => l.ReturnDate == null).CountAsync();
            
            ViewBag.TotalBooks = totalStock + activeLoanCount; // Mevcut stok + ödünç verilen
            ViewBag.AvailableBooks = totalStock; // Sadece rafta olan
            
            // Kullanıcının aktif ödünç sayısı
            ViewBag.MyActiveLoans = await _context.Loans
                .Where(l => l.MemberId == user.MemberId && l.ReturnDate == null)
                .CountAsync();

            // Kullanıcının bekleyen istek sayısı
            ViewBag.MyPendingRequests = await _context.LoanRequests
                .Where(lr => lr.RequestedByUserId == userId && lr.RequestStatus == RequestStatus.Pending)
                .CountAsync();

            // Kullanıcının son istekleri
            var recentRequests = await _context.LoanRequests
                .Include(lr => lr.Book)
                .Where(lr => lr.RequestedByUserId == userId)
                .OrderByDescending(lr => lr.RequestDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentRequests = recentRequests;

            // Kullanıcının aktif ödünçleri
            var activeLoans = await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.MemberId == user.MemberId && l.ReturnDate == null)
                .OrderBy(l => l.DueDate)
                .ToListAsync();

            ViewBag.ActiveLoans = activeLoans;

            return View(user);
        }
    }
}

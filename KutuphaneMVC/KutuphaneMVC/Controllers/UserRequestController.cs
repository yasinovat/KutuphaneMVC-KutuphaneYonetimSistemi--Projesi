using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using KutuphaneMVC.Filters;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("User")]
    public class UserRequestController : Controller
    {
        private readonly LibraryContext _context;

        public UserRequestController(LibraryContext context)
        {
            _context = context;
        }

        // GET: UserRequest/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var requests = await _context.LoanRequests
                .Include(lr => lr.Book)
                .Include(lr => lr.RequestedForMember)
                .Include(lr => lr.ProcessedByUser)
                .Where(lr => lr.RequestedByUserId == userId)
                .OrderByDescending(lr => lr.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // POST: UserRequest/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var request = await _context.LoanRequests
                .FirstOrDefaultAsync(lr => lr.RequestId == id && lr.RequestedByUserId == userId);

            if (request == null)
            {
                TempData["Error"] = "İstek bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Sadece bekleyen istekler iptal edilebilir
            if (request.RequestStatus != RequestStatus.Pending)
            {
                TempData["Error"] = "Sadece bekleyen istekler iptal edilebilir.";
                return RedirectToAction(nameof(Index));
            }

            request.RequestStatus = RequestStatus.Cancelled;
            request.ProcessedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "İstek başarıyla iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: UserRequest/MyLoans
        public async Task<IActionResult> MyLoans()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.MemberId == null)
            {
                TempData["Error"] = "Kullanıcı bilgileri bulunamadı.";
                return RedirectToAction("Index", "UserDashboard");
            }

            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.MemberId == user.MemberId)
                .OrderByDescending(l => l.BorrowDate)
                .ToListAsync();

            return View(loans);
        }
    }
}

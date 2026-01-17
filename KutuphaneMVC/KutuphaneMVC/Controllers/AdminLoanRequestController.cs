using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using KutuphaneMVC.Filters;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminLoanRequestController : Controller
    {
        private readonly LibraryContext _context;

        public AdminLoanRequestController(LibraryContext context)
        {
            _context = context;
        }

        // GET: AdminLoanRequest/Index
        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.LoanRequests
                .Include(lr => lr.Book)
                .Include(lr => lr.RequestedByUser)
                .Include(lr => lr.RequestedForMember)
                .Include(lr => lr.ProcessedByUser)
                .AsQueryable();

            // Durum filtresi
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<RequestStatus>(status, out var requestStatus))
                {
                    query = query.Where(lr => lr.RequestStatus == requestStatus);
                }
            }
            else
            {
                // Varsayılan olarak bekleyen istekleri göster
                query = query.Where(lr => lr.RequestStatus == RequestStatus.Pending);
            }

            var requests = await query
                .OrderByDescending(lr => lr.RequestDate)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(requests);
        }

        // POST: AdminLoanRequest/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var adminUserId = HttpContext.Session.GetInt32("userId");
            if (adminUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var request = await _context.LoanRequests
                .Include(lr => lr.Book)
                .Include(lr => lr.RequestedForMember)
                .FirstOrDefaultAsync(lr => lr.RequestId == id);

            if (request == null)
            {
                TempData["Error"] = "İstek bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (request.RequestStatus != RequestStatus.Pending)
            {
                TempData["Error"] = "Bu istek zaten işlenmiş.";
                return RedirectToAction(nameof(Index));
            }

            // Stok kontrolü
            if (request.Book == null || request.Book.Stock <= 0)
            {
                TempData["Error"] = "Kitap stokta yok. İstek reddedilebilir.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. İsteği onayla
                request.RequestStatus = RequestStatus.Approved;
                request.ProcessedByUserId = adminUserId;
                request.ProcessedDate = DateTime.Now;

                // 2. Loan kaydı oluştur
                var loan = new Loan
                {
                    BookId = request.BookId,
                    MemberId = request.RequestedForMemberId,
                    BorrowDate = DateTime.Now,
                    DueDate = request.RequestedDueDate,
                    ReturnDate = null
                };

                _context.Loans.Add(loan);

                // 3. Kitap stokunu azalt
                request.Book.Stock--;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"İstek onaylandı ve ödünç kaydı oluşturuldu. Kitap: {request.Book.Title}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "İstek onaylanırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminLoanRequest/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminNotes)
        {
            var adminUserId = HttpContext.Session.GetInt32("userId");
            if (adminUserId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var request = await _context.LoanRequests.FindAsync(id);
            if (request == null)
            {
                TempData["Error"] = "İstek bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (request.RequestStatus != RequestStatus.Pending)
            {
                TempData["Error"] = "Bu istek zaten işlenmiş.";
                return RedirectToAction(nameof(Index));
            }

            request.RequestStatus = RequestStatus.Rejected;
            request.ProcessedByUserId = adminUserId;
            request.ProcessedDate = DateTime.Now;
            request.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "İstek reddedildi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminLoanRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.LoanRequests
                .Include(lr => lr.Book)
                .Include(lr => lr.RequestedByUser)
                    .ThenInclude(u => u!.Member)
                .Include(lr => lr.RequestedForMember)
                .Include(lr => lr.ProcessedByUser)
                .FirstOrDefaultAsync(lr => lr.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using KutuphaneMVC.Filters;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthorizeRole("Admin")]
    public class UserManagementController : Controller
    {
        private readonly LibraryContext _context;

        public UserManagementController(LibraryContext context)
        {
            _context = context;
        }

        // GET: UserManagement/Index
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Member)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            return View(users);
        }

        // GET: UserManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password, string email, string role, string? fullName, string? phone)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Bu email adresi zaten kullanılıyor!";
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                User user;
                
                // Eğer User rolündeyse Member oluştur
                if (role == "User" && !string.IsNullOrEmpty(fullName))
                {
                    var member = new Member
                    {
                        FullName = fullName,
                        Email = email,
                        Phone = phone
                    };

                    _context.Members.Add(member);
                    await _context.SaveChangesAsync();

                    user = new User
                    {
                        Username = username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Email = email,
                        Role = UserRole.User,
                        MemberId = member.MemberId,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                }
                else
                {
                    // Admin için Member gerekmez
                    user = new User
                    {
                        Username = username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Email = email,
                        Role = role == "Admin" ? UserRole.Admin : UserRole.User,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ViewBag.Error = "Kullanıcı oluşturulurken bir hata oluştu.";
                return View();
            }
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string email, bool isActive, string? fullName, string? phone)
        {
            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Email değişikliği kontrolü
            if (user.Email != email && await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id))
            {
                ViewBag.Error = "Bu email adresi zaten kullanılıyor!";
                return View(user);
            }

            user.Email = email;
            user.IsActive = isActive;

            // Eğer Member varsa güncelle
            if (user.Member != null && !string.IsNullOrEmpty(fullName))
            {
                user.Member.FullName = fullName;
                user.Member.Email = email;
                user.Member.Phone = phone;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: UserManagement/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Kullanıcı {(user.IsActive ? "aktif" : "pasif")} hale getirildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: UserManagement/ResetPassword/5
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (newPassword.Length < 8)
            {
                TempData["Error"] = "Şifre en az 8 karakter olmalıdır!";
                return RedirectToAction(nameof(Edit), new { id });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şifre başarıyla sıfırlandı.";
            return RedirectToAction(nameof(Index));
        }
    }
}

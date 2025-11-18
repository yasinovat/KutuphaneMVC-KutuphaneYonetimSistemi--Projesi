using KutuphaneMVC.Filters;
using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthFilter]
    public class MemberController : Controller
    {
        private readonly LibraryContext _context;

        public MemberController(LibraryContext context)
        {
            _context = context;
        }

        // 1. Üye Listesi
        public IActionResult Index()
        {
            var uyeler = _context.Members.ToList();
            return View(uyeler);
        }

        // 2. Üye Ekle (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. Üye Ekle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Members.Add(member);
                _context.SaveChanges();
                TempData["Success"] = "Üye başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // 4. Üye Düzenle (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var uye = _context.Members.Find(id);
            if (uye == null)
                return NotFound();
            return View(uye);
        }

        // 5. Üye Düzenle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Members.Update(member);
                _context.SaveChanges();
                TempData["Success"] = "Üye bilgileri güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // 6. Üye Sil (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var uye = _context.Members.Find(id);
            if (uye == null)
                return NotFound();
            return View(uye);
        }

        // 7. Üye Sil (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var uye = _context.Members.Find(id);
            if (uye != null)
            {
                _context.Members.Remove(uye);
                _context.SaveChanges();
                TempData["Success"] = "Üye başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

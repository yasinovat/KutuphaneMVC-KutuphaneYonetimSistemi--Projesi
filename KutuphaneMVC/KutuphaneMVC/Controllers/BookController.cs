using KutuphaneMVC.Models;
using Microsoft.AspNetCore.Mvc;
using KutuphaneMVC.Filters;
using System.Net.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Controllers
{
    [AuthFilter]
    public class BookController : Controller
    {
        private readonly LibraryContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookController(LibraryContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index(string searchTitle, string searchAuthor, string searchGenre)
        {
            var books = _context.Books.AsQueryable();

            // Filtreleme işlemleri
            if (!string.IsNullOrEmpty(searchTitle))
                books = books.Where(b => b.Title.Contains(searchTitle));

            if (!string.IsNullOrEmpty(searchAuthor))
                books = books.Where(b => b.Author.Contains(searchAuthor));

            if (!string.IsNullOrEmpty(searchGenre))
                books = books.Where(b => b.Genre.Contains(searchGenre));

            return View(books.ToList());
        }

        // Yeni kitap ekleme formu
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kitap kaydetme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid)
                return View(book);

            string query = $"{book.Title} {book.Author}";
            string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(stream);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var firstItem = items[0];
                    if (firstItem.TryGetProperty("volumeInfo", out var volumeInfo) &&
                        volumeInfo.TryGetProperty("imageLinks", out var imageLinks) &&
                        imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                    {
                        book.ImageUrl = thumbnail.GetString();
                    }
                }
            }

            book.ImageUrl ??= "https://via.placeholder.com/400x260?text=Kapak+Yok";

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitap başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // Kitap düzenleme (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        //  Kitap düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.BookId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(book);

            var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
            if (existingBook == null)
                return NotFound();

            // Eğer yeni resim boşsa, eski resmi koru
            if (string.IsNullOrEmpty(book.ImageUrl))
                book.ImageUrl = existingBook.ImageUrl;

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kitap başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(b => b.BookId == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // Kitap silme (GET - onay sayfası)
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        // Kitap silme (POST - onaylandıktan sonra)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kitap başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}



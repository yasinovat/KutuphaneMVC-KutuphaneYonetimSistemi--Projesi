using KutuphaneMVC.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//  Veritabanı bağlantısı
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KutuphaneDb")));

// MVC servisleri
builder.Services.AddControllersWithViews();

// Session servisini ekle. 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dk boyunca aktif oturum
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Google Books API'den veri çekebilmek için HttpClient servisini ekle
builder.Services.AddHttpClient();

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware zinciri
app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot/css erişim

app.UseRouting();

// Session middleware aktif
app.UseSession();

app.UseAuthorization();

// Varsayılan rota → Login ekranı
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();



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

// Seed admin user if not exists
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    
    try
    {
        // Veritabanını oluştur (yoksa)
        context.Database.EnsureCreated();
        
        // Admin kullanıcısı var mı kontrol et
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@kutuphane.com",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            
            context.Users.Add(adminUser);
            context.SaveChanges();
            
            Console.WriteLine("✅ Admin kullanıcısı oluşturuldu (admin / admin123)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Seed hatası: {ex.Message}");
    }
}

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



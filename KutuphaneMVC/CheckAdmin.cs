using Microsoft.EntityFrameworkCore;
using KutuphaneMVC.Models;

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=KutuphaneYonetimSistemi;Trusted_Connection=True;TrustServerCertificate=True;");

using var context = new LibraryContext(optionsBuilder.Options);

// Admin kullanıcısını kontrol et
var admin = context.Users.FirstOrDefault(u => u.Username == "admin");

if (admin == null)
{
    Console.WriteLine("❌ Admin kullanıcısı bulunamadı!");
    Console.WriteLine("Yeni admin kullanıcısı oluşturuluyor...");
    
    var newAdmin = new User
    {
        Username = "admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
        Email = "admin@kutuphane.com",
        Role = UserRole.Admin,
        IsActive = true,
        CreatedDate = DateTime.Now
    };
    
    context.Users.Add(newAdmin);
    context.SaveChanges();
    
    Console.WriteLine("✅ Admin kullanıcısı oluşturuldu!");
    Console.WriteLine("   Kullanıcı adı: admin");
    Console.WriteLine("   Şifre: admin123");
}
else
{
    Console.WriteLine($"✅ Admin kullanıcısı bulundu:");
    Console.WriteLine($"   Username: {admin.Username}");
    Console.WriteLine($"   Email: {admin.Email}");
    Console.WriteLine($"   Role: {admin.Role}");
    Console.WriteLine($"   IsActive: {admin.IsActive}");
    Console.WriteLine($"   CreatedDate: {admin.CreatedDate}");
    
    // Şifre kontrolü
    bool passwordCorrect = BCrypt.Net.BCrypt.Verify("admin123", admin.PasswordHash);
    Console.WriteLine($"   Şifre doğrulaması (admin123): {(passwordCorrect ? "✅ DOĞRU" : "❌ YANLIŞ")}");
    
    if (!passwordCorrect)
    {
        Console.WriteLine("\n⚠️ Şifre yanlış, yeniden ayarlanıyor...");
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        context.SaveChanges();
        Console.WriteLine("✅ Şifre admin123 olarak sıfırlandı!");
    }
}

Console.WriteLine("\n📊 Tüm kullanıcılar:");
var allUsers = context.Users.ToList();
foreach (var user in allUsers)
{
    Console.WriteLine($"   - {user.Username} ({user.Role}) - Active: {user.IsActive}");
}

using Microsoft.EntityFrameworkCore;
using KutuphaneMVC.Models;

Console.WriteLine("=== Admin Login Testi ===\n");

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=KutuphaneYonetimSistemi;Trusted_Connection=True;TrustServerCertificate=True;");

using var context = new LibraryContext(optionsBuilder.Options);

// Admin kullanıcısını bul
var admin = context.Users.FirstOrDefault(u => u.Username == "admin");

if (admin == null)
{
    Console.WriteLine("❌ Admin kullanıcısı bulunamadı!");
    return;
}

Console.WriteLine($"✅ Admin kullanıcısı bulundu:");
Console.WriteLine($"   Username: {admin.Username}");
Console.WriteLine($"   Email: {admin.Email}");
Console.WriteLine($"   Role: {admin.Role}");
Console.WriteLine($"   IsActive: {admin.IsActive}");
Console.WriteLine($"\n🔑 Şifre Hash: {admin.PasswordHash}\n");

// Şifreleri test et
string[] testPasswords = { "admin123", "Admin123", "admin", "ADMIN123" };

foreach (var password in testPasswords)
{
    bool isValid = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
    string result = isValid ? "✅ DOĞRU" : "❌ YANLIŞ";
    Console.WriteLine($"   {password.PadRight(15)} → {result}");
}

// Eğer admin123 geçmiyorsa, şifreyi sıfırla
bool admin123Valid = BCrypt.Net.BCrypt.Verify("admin123", admin.PasswordHash);
if (!admin123Valid)
{
    Console.WriteLine("\n⚠️ Şifre yanlış! Şifre admin123 olarak sıfırlanıyor...");
    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
    context.SaveChanges();
    Console.WriteLine("✅ Şifre başarıyla sıfırlandı!");
    
    // Tekrar test et
    admin = context.Users.FirstOrDefault(u => u.Username == "admin");
    bool newPasswordValid = BCrypt.Net.BCrypt.Verify("admin123", admin.PasswordHash!);
    Console.WriteLine($"   Yeni şifre testi: {(newPasswordValid ? "✅ BAŞARILI" : "❌ BAŞARISIZ")}");
}
else
{
    Console.WriteLine("\n✅ Şifre doğru! Giriş yapabilirsiniz:");
    Console.WriteLine("   Username: admin");
    Console.WriteLine("   Password: admin123");
}

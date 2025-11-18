using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Models
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } //DBsets
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace KutuphaneMVC.Models
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoanRequest> LoanRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Member ilişkisi (1-to-0..1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Member)
                .WithMany()
                .HasForeignKey(u => u.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanRequest - User (RequestedBy) ilişkisi
            modelBuilder.Entity<LoanRequest>()
                .HasOne(lr => lr.RequestedByUser)
                .WithMany(u => u.RequestedLoans)
                .HasForeignKey(lr => lr.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanRequest - User (ProcessedBy) ilişkisi
            modelBuilder.Entity<LoanRequest>()
                .HasOne(lr => lr.ProcessedByUser)
                .WithMany(u => u.ProcessedLoans)
                .HasForeignKey(lr => lr.ProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanRequest - Member ilişkisi
            modelBuilder.Entity<LoanRequest>()
                .HasOne(lr => lr.RequestedForMember)
                .WithMany()
                .HasForeignKey(lr => lr.RequestedForMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanRequest - Book ilişkisi
            modelBuilder.Entity<LoanRequest>()
                .HasOne(lr => lr.Book)
                .WithMany()
                .HasForeignKey(lr => lr.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Username unique constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Email unique constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}

using FinalProjectV3.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProjectV3.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Accountant> Accountants { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<AccountantCredentials> AccountantCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Accountant>()
                .HasMany(a => a.Users)
                .WithOne(u => u.Accountant) 
                .HasForeignKey(u => u.AccountantId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanType)
                .HasConversion<string>(); 

            modelBuilder.Entity<Loan>()
                .Property(l => l.Currency)
                .HasConversion<string>(); 

            modelBuilder.Entity<Loan>()
                .Property(l => l.Period)
                .HasConversion<string>(); 

            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanStatus)
                .HasConversion<string>();
            modelBuilder.Entity<User>()
                .HasOne(u => u.Credentials)
                .WithOne()
                .HasForeignKey<User>(u => u.CredentialsId);
            modelBuilder.Entity<Accountant>()
                .HasOne(a => a.AccountantCredentials)
                .WithOne(ac => ac.Accountant)
                .HasForeignKey<Accountant>(a => a.AccountantCredentialsId);
            modelBuilder.Entity<Loan>()
                .Property(l => l.Amount)
                .HasColumnType("decimal(18, 2)");
        }
    }
}

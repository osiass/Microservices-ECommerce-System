using Microsoft.EntityFrameworkCore;
using Payment.API.Entities;

namespace Payment.API.Data
{
    public class PaymentContext : DbContext
    {
        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options) { }
        public DbSet<PaymentTransaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentTransaction>().ToTable("Transactions");
        }
    }
}

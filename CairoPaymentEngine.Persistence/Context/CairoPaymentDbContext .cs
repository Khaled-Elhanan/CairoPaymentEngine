using CairoPaymentEngine.Domain.Common;
using CairoPaymentEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CairoPaymentEngine.Persistence.Context
{
    public class CairoPaymentDbContext:DbContext
    {
        public CairoPaymentDbContext(DbContextOptions<CairoPaymentDbContext> options) : base(options)
        {
        }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ExternalId)
                .IsUnique();
            modelBuilder.Ignore<DomainEvent>();

            base.OnModelCreating(modelBuilder);
        }
    }
}

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
            .Property(p => p.ExternalId)
            .HasColumnType("nvarchar(MAX)");

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                modelBuilder.Entity<Order>()
                    .Property(o => o.Id)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Order>()
                    .Property(o => o.Amount)
                    .HasColumnType("REAL");

                modelBuilder.Entity<Order>()
                    .Property(o => o.Currency)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Payment>()
                    .Property(p => p.Id)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Payment>()
                    .Property(p => p.OrderId)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Payment>()
                    .Property(p => p.ExternalId)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Payment>()
                    .Property(p => p.IdempotencyKey)
                    .HasColumnType("TEXT");

                modelBuilder.Entity<Payment>()
                    .Property(p => p.ProcessedEventId)
                    .HasColumnType("TEXT");
            }



            modelBuilder.Ignore<DomainEvent>();

            base.OnModelCreating(modelBuilder);
        }
    }
}

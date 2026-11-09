using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Models;

namespace RecyclingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PickupRequest> PickupRequests { get; set; }
        public DbSet<RequestItem> RequestItems { get; set; }
        public DbSet<WasteType> WasteTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PickupRequest>().Property(p => p.TotalValue).HasPrecision(18, 2);
            builder.Entity<RequestItem>().Property(r => r.QuantityKg).HasPrecision(18, 2);
            builder.Entity<WasteType>().Property(w => w.PricePerKg).HasPrecision(18, 2);

            builder.Entity<PickupRequest>()
                .HasOne(p => p.User)
                .WithMany(u => u.PickupRequests)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PickupRequest>()
                .HasOne(p => p.Collector)
                .WithMany(u => u.CollectedRequests)
                .HasForeignKey(p => p.CollectorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequestItem>()
                .HasOne(r => r.PickupRequest)
                .WithMany(p => p.RequestItems)
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequestItem>()
                .HasOne(r => r.WasteType)
                .WithMany(w => w.RequestItems)
                .HasForeignKey(r => r.WasteTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WasteType>().HasData(
                new WasteType { Id = 1, Name = "Plastic", PricePerKg = 15.00m },
                new WasteType { Id = 2, Name = "Paper", PricePerKg = 8.00m },
                new WasteType { Id = 3, Name = "Metals", PricePerKg = 25.00m },
                new WasteType { Id = 4, Name = "Glass", PricePerKg = 5.00m },
                new WasteType { Id = 5, Name = "Electronics", PricePerKg = 40.00m }
            );
        }
    }
}
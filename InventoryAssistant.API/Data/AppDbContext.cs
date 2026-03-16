using Microsoft.EntityFrameworkCore;
using InventoryAssistant.API.Models;

namespace InventoryAssistant.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitHistory> UnitHistory { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<OrderBatch> OrderBatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique barcode
            modelBuilder.Entity<Unit>()
                .HasIndex(u => u.Barcode)
                .IsUnique();

            // Unit → History
            modelBuilder.Entity<UnitHistory>()
                .HasOne(h => h.Unit)
                .WithMany(u => u.History)
                .HasForeignKey(h => h.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unit → Parts
            modelBuilder.Entity<Part>()
                .HasOne(p => p.Unit)
                .WithMany(u => u.Parts)
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderBatch → Parts
            modelBuilder.Entity<Part>()
                .HasOne(p => p.OrderBatch)
                .WithMany(o => o.Parts)
                .HasForeignKey(p => p.OrderBatchId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

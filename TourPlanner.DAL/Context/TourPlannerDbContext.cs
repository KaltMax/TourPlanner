using Microsoft.EntityFrameworkCore;
using TourPlanner.DAL.Entities;

namespace TourPlanner.DAL.Context
{
    public class TourPlannerDbContext : DbContext
    {
        public DbSet<TourEntity> Tours { get; set; }
        public DbSet<TourLogEntity> TourLogs { get; set; }

        public TourPlannerDbContext(DbContextOptions<TourPlannerDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set schema for all tables
            modelBuilder.HasDefaultSchema("tourplanner");

            // Configure Tour entity
            modelBuilder.Entity<TourEntity>(entity =>
            {
                entity.ToTable("Tours");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.From).IsRequired().HasMaxLength(100);
                entity.Property(e => e.To).IsRequired().HasMaxLength(100);
                entity.Property(e => e.GeoJson).IsRequired();
                entity.Property(e => e.Directions).IsRequired();
                entity.Property(e => e.Distance).IsRequired();
                entity.Property(e => e.EstimatedTime).IsRequired();
                entity.Property(e => e.TransportType).IsRequired();

                // Define index for faster querying
                entity.HasIndex(e => e.Name);
            });

            // Configure TourLog entity
            modelBuilder.Entity<TourLogEntity>(entity =>
            {
                entity.ToTable("TourLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.TotalDistance).IsRequired();
                entity.Property(e => e.Difficulty).IsRequired();
                entity.Property(e => e.Rating).IsRequired();

                // Configure relationship with Tour (one-to-many)
                entity.HasOne(t => t.Tour)
                    .WithMany(t => t.TourLogs)
                    .HasForeignKey(t => t.TourId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

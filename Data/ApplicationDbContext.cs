using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManagementSystem.Models;

namespace StockManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<StockItem>()
                .HasOne(s => s.Category)
                .WithMany(c => c.StockItems)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockItem>()
                .HasOne(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockItem>()
                .HasOne(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Furniture", Description = "Office and home furniture", CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Electronics", Description = "Electronic devices and equipment", CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Goods", Description = "General goods and supplies", CreatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Technology", Description = "Technology and IT equipment", CreatedAt = DateTime.Now }
            );
        }
    }
} 
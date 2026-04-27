using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Rating)
            .HasPrecision(2, 1);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.FullName)
            .HasMaxLength(100);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Email)
            .HasMaxLength(150);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.PasswordHash)
            .HasMaxLength(500);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Role)
            .HasMaxLength(30);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}

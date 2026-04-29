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
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

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

        var appUser = modelBuilder.Entity<AppUser>();

        appUser.Property(u => u.FullName)
            .HasMaxLength(100);

        appUser.Property(u => u.Email)
            .HasMaxLength(150);

        appUser.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        appUser.Property(u => u.Role)
            .HasMaxLength(30);

        appUser.HasIndex(u => u.Email)
            .IsUnique();

        var cart = modelBuilder.Entity<Cart>();

        cart.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        cart.HasIndex(c => c.UserId)
            .IsUnique();

        var cartItem = modelBuilder.Entity<CartItem>();

        cartItem.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        cartItem.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        cartItem.HasIndex(ci => new { ci.CartId, ci.ProductId })
            .IsUnique();
    }
}

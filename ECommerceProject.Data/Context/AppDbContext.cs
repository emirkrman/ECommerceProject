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
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserPaymentCard> UserPaymentCards => Set<UserPaymentCard>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

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

        var userAddress = modelBuilder.Entity<UserAddress>();

        userAddress.Property(a => a.Title)
            .HasMaxLength(80);

        userAddress.Property(a => a.FullName)
            .HasMaxLength(100);

        userAddress.Property(a => a.PhoneNumber)
            .HasMaxLength(30);

        userAddress.Property(a => a.City)
            .HasMaxLength(80);

        userAddress.Property(a => a.District)
            .HasMaxLength(80);

        userAddress.Property(a => a.AddressLine)
            .HasMaxLength(500);

        userAddress.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        userAddress.HasIndex(a => new { a.UserId, a.Title });

        var userPaymentCard = modelBuilder.Entity<UserPaymentCard>();

        userPaymentCard.Property(c => c.CardHolderName)
            .HasMaxLength(100);

        userPaymentCard.Property(c => c.CardNumber)
            .HasMaxLength(19);

        userPaymentCard.Property(c => c.ExpiryMonth)
            .HasMaxLength(2);

        userPaymentCard.Property(c => c.ExpiryYear)
            .HasMaxLength(4);

        userPaymentCard.Property(c => c.Cvv)
            .HasMaxLength(3);

        userPaymentCard.Property(c => c.CardLastFour)
            .HasMaxLength(4);

        userPaymentCard.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        userPaymentCard.HasIndex(c => new { c.UserId, c.CardLastFour });

        var order = modelBuilder.Entity<Order>();

        order.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        order.Property(o => o.PaymentMessage)
            .HasMaxLength(250);

        order.Property(o => o.PaymentCardLastFour)
            .HasMaxLength(4);

        order.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        order.HasOne(o => o.UserAddress)
            .WithMany()
            .HasForeignKey(o => o.UserAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        var orderItem = modelBuilder.Entity<OrderItem>();

        orderItem.Property(oi => oi.ProductName)
            .HasMaxLength(200);

        orderItem.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        orderItem.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        orderItem.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

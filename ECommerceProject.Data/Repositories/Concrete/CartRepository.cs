using ECommerceProject.Data.Context;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Data.Repositories.Concrete;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetTrackedByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public void Remove(Cart cart)
    {
        _context.Carts.Remove(cart);
    }

    public void RemoveItem(CartItem cartItem)
    {
        _context.CartItems.Remove(cartItem);
    }
}

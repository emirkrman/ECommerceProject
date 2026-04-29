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

    public async Task<List<Cart>> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
            .AsNoTracking()
            .Include(c => c.Product)
            .ThenInclude(p => p!.Category)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Cart>> GetTrackedByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<Cart?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public void Remove(Cart cart)
    {
        _context.Carts.Remove(cart);
    }

    public void RemoveRange(IEnumerable<Cart> carts)
    {
        _context.Carts.RemoveRange(carts);
    }
}

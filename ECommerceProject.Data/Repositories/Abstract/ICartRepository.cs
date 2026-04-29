using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Data.Repositories.Abstract;

public interface ICartRepository
{
    Task<List<Cart>> GetByUserIdAsync(int userId);
    Task<List<Cart>> GetTrackedByUserIdAsync(int userId);
    Task<Cart?> GetByUserAndProductAsync(int userId, int productId);
    Task AddAsync(Cart cart);
    void Remove(Cart cart);
    void RemoveRange(IEnumerable<Cart> carts);
}

using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Data.Repositories.Abstract;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart?> GetTrackedByUserIdAsync(int userId);
    Task<CartItem?> GetItemByUserAndProductAsync(int userId, int productId);
    Task AddAsync(Cart cart);
    void Remove(Cart cart);
    void RemoveItem(CartItem cartItem);
    void RemoveItems(IEnumerable<CartItem> cartItems);
}

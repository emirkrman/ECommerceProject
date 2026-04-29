using ECommerceProject.Business.Models.Carts;
using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Abstract;

public interface ICartService
{
    Task<List<Cart>> GetUserCartAsync(int userId);
    Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity);
    Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}

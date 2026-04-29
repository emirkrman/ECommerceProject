using ECommerceProject.Business.Models.Carts;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Data.UnitOfWork;
using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Concrete;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Cart>> GetUserCartAsync(int userId)
    {
        return await _cartRepository.GetByUserIdAsync(userId);
    }

    public async Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
            return CartOperationResult.Failure("Adet 1 veya daha buyuk olmalidir.");

        var product = await _productRepository.GetActiveDetailsAsync(productId);
        if (product == null)
            return CartOperationResult.Failure("Urun bulunamadi.");

        if (product.Stock <= 0)
            return CartOperationResult.Failure("Bu urun stokta yok.");

        var cartItem = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        var newQuantity = quantity + (cartItem?.Quantity ?? 0);

        if (newQuantity > product.Stock)
            return CartOperationResult.Failure("Secilen adet urun stok miktarini asiyor.");

        if (cartItem == null)
        {
            await _cartRepository.AddAsync(new Cart
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity,
                CreatedDate = DateTime.UtcNow
            });
        }
        else
        {
            cartItem.Quantity = newQuantity;
            cartItem.UpdatedDate = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return CartOperationResult.Success("Urun sepete eklendi.");
    }

    public async Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var cartItem = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        if (cartItem == null)
            return CartOperationResult.Failure("Sepet urunu bulunamadi.");

        if (quantity <= 0)
        {
            _cartRepository.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return CartOperationResult.Success("Urun sepetten kaldirildi.");
        }

        var product = await _productRepository.GetActiveDetailsAsync(productId);
        if (product == null)
            return CartOperationResult.Failure("Urun bulunamadi.");

        if (quantity > product.Stock)
            return CartOperationResult.Failure("Secilen adet urun stok miktarini asiyor.");

        cartItem.Quantity = quantity;
        cartItem.UpdatedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return CartOperationResult.Success("Sepet guncellendi.");
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var cartItem = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        if (cartItem == null)
            return false;

        _cartRepository.Remove(cartItem);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(int userId)
    {
        var cartItems = await _cartRepository.GetTrackedByUserIdAsync(userId);
        _cartRepository.RemoveRange(cartItems);
        await _unitOfWork.SaveChangesAsync();
    }
}

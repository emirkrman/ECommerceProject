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
    private readonly IStockReservationService _stockReservationService;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IStockReservationService stockReservationService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _stockReservationService = stockReservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Cart?> GetUserCartAsync(int userId)
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

        var cart = await GetOrCreateTrackedCartAsync(userId);
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        var newQuantity = quantity + (cartItem?.Quantity ?? 0);

        var reserved = await _stockReservationService.TryReserveAsync(userId, productId, newQuantity, product.Stock);
        if (!reserved)
            return CartOperationResult.Failure("Secilen adet urun stok miktarini asiyor.");

        if (cartItem == null)
        {
            cart.Items.Add(new CartItem
            {
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

        cart.UpdatedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return CartOperationResult.Success("Urun sepete eklendi.");
    }

    public async Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var cart = await _cartRepository.GetTrackedByUserIdAsync(userId);
        if (cart == null)
            return CartOperationResult.Failure("Sepet urunu bulunamadi.");

        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem == null)
            return CartOperationResult.Failure("Sepet urunu bulunamadi.");

        if (quantity <= 0)
        {
            RemoveCartItemOrCart(cart, cartItem);
            await _unitOfWork.SaveChangesAsync();
            await _stockReservationService.ReleaseAsync(userId, productId);
            return CartOperationResult.Success("Urun sepetten kaldirildi.");
        }

        var product = await _productRepository.GetActiveDetailsAsync(productId);
        if (product == null)
            return CartOperationResult.Failure("Urun bulunamadi.");

        var reserved = await _stockReservationService.TryReserveAsync(userId, productId, quantity, product.Stock);
        if (!reserved)
            return CartOperationResult.Failure("Secilen adet urun stok miktarini asiyor.");

        cartItem.Quantity = quantity;
        cartItem.UpdatedDate = DateTime.UtcNow;
        cart.UpdatedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return CartOperationResult.Success("Sepet guncellendi.");
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await _cartRepository.GetTrackedByUserIdAsync(userId);
        if (cart == null)
            return false;

        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem == null)
            return false;

        RemoveCartItemOrCart(cart, cartItem);
        await _unitOfWork.SaveChangesAsync();
        await _stockReservationService.ReleaseAsync(userId, productId);
        return true;
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepository.GetTrackedByUserIdAsync(userId);
        if (cart == null)
            return;

        var productIds = cart.Items.Select(item => item.ProductId).ToList();

        _cartRepository.Remove(cart);
        await _unitOfWork.SaveChangesAsync();
        await _stockReservationService.ReleaseManyAsync(userId, productIds);
    }

    private async Task<Cart> GetOrCreateTrackedCartAsync(int userId)
    {
        var cart = await _cartRepository.GetTrackedByUserIdAsync(userId);
        if (cart != null)
            return cart;

        cart = new Cart
        {
            UserId = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _cartRepository.AddAsync(cart);
        return cart;
    }

    private void RemoveCartItemOrCart(Cart cart, CartItem cartItem)
    {
        if (cart.Items.Count <= 1)
        {
            _cartRepository.Remove(cart);
            return;
        }

        _cartRepository.RemoveItem(cartItem);
        cart.UpdatedDate = DateTime.UtcNow;
    }
}

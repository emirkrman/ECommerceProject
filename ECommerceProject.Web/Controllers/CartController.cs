using ECommerceProject.Business.Models.Carts;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceProject.Web.Controllers;

[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService, INavigationService navigationService)
        : base(navigationService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        return View(await _cartService.GetUserCartAsync(userId.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var result = await _cartService.AddToCartAsync(userId.Value, productId, quantity);
        SetCartMessage(result.Succeeded, result.Message);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var result = await _cartService.UpdateQuantityAsync(userId.Value, productId, quantity);

        if (IsAjaxRequest())
            return await CartJsonResultAsync(userId.Value, productId, result);

        SetCartMessage(result.Succeeded, result.Message);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var removed = await _cartService.RemoveFromCartAsync(userId.Value, productId);

        if (IsAjaxRequest())
        {
            var result = removed
                ? CartOperationResult.Success("Ürün sepetten kaldırıldı.")
                : CartOperationResult.Failure("Sepet ürünü bulunamadı.");

            return await CartJsonResultAsync(userId.Value, productId, result);
        }

        if (removed)
            TempData["CartSuccess"] = "Ürün sepetten kaldırıldı.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        await _cartService.ClearCartAsync(userId.Value);

        if (IsAjaxRequest())
        {
            return Json(new
            {
                succeeded = true,
                message = "Sepet temizlendi.",
                messageType = "success",
                cartTotal = 0m.ToString("C"),
                isEmpty = true
            });
        }

        TempData["CartSuccess"] = "Sepet temizlendi.";

        return RedirectToAction(nameof(Index));
    }

    private void SetCartMessage(bool succeeded, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        TempData[succeeded ? "CartSuccess" : "CartError"] = message;
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    private async Task<IActionResult> CartJsonResultAsync(
        int userId,
        int productId,
        CartOperationResult result)
    {
        var cart = await _cartService.GetUserCartAsync(userId);
        var cartItems = cart?.Items ?? new List<CartItem>();
        var cartItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
        var cartTotal = cartItems
            .Where(item => item.Product != null)
            .Sum(item => item.Product!.Price * item.Quantity);

        return Json(new
        {
            succeeded = result.Succeeded,
            message = result.Message,
            messageType = result.Succeeded ? "success" : "danger",
            productId,
            removed = cartItem == null,
            quantity = cartItem?.Quantity ?? 0,
            itemSubtotal = cartItem?.Product == null
                ? 0m.ToString("C")
                : (cartItem.Product.Price * cartItem.Quantity).ToString("C"),
            cartTotal = cartTotal.ToString("C"),
            isEmpty = !cartItems.Any()
        });
    }
}

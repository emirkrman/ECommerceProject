using ECommerceProject.Business.Services.Abstract;
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

        if (await _cartService.RemoveFromCartAsync(userId.Value, productId))
            TempData["CartSuccess"] = "Urun sepetten kaldirildi.";

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
        TempData["CartSuccess"] = "Sepet temizlendi.";

        return RedirectToAction(nameof(Index));
    }

    private void SetCartMessage(bool succeeded, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        TempData[succeeded ? "CartSuccess" : "CartError"] = message;
    }
}

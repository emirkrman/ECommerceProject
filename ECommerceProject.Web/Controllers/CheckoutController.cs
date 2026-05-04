using ECommerceProject.Business.Models.Checkout;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Web.ViewModels.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceProject.Web.Controllers;

[Authorize]
public class CheckoutController : BaseController
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(
        ICheckoutService checkoutService,
        INavigationService navigationService)
        : base(navigationService)
    {
        _checkoutService = checkoutService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var model = await BuildViewModelAsync(userId.Value);
        if (!model.Items.Any())
        {
            TempData["CartError"] = "Ödeme için sepetinizde ürün bulunmuyor.";
            return RedirectToAction(nameof(CartController.Index), "Cart");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(CheckoutViewModel model)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        ValidateAddress(model);
        ValidatePaymentCard(model);

        if (!ModelState.IsValid)
        {
            var summary = await BuildViewModelAsync(userId.Value);
            model.Items = summary.Items;
            model.SavedAddresses = summary.SavedAddresses;
            model.SavedCards = summary.SavedCards;
            return View(nameof(Index), model);
        }

        var result = await _checkoutService.PlaceOrderAsync(userId.Value, new CheckoutRequest
        {
            SelectedAddressId = model.SelectedAddressId,
            SelectedCardId = model.SelectedCardId,
            SaveCard = model.SaveCard,
            AddressTitle = model.AddressTitle ?? string.Empty,
            FullName = model.FullName ?? string.Empty,
            PhoneNumber = model.PhoneNumber ?? string.Empty,
            City = model.City ?? string.Empty,
            District = model.District ?? string.Empty,
            AddressLine = model.AddressLine ?? string.Empty,
            CardHolderName = model.CardHolderName ?? string.Empty,
            CardNumber = model.CardNumber ?? string.Empty,
            ExpiryMonth = model.ExpiryMonth ?? string.Empty,
            ExpiryYear = model.ExpiryYear ?? string.Empty,
            Cvv = model.Cvv ?? string.Empty
        });

        return View("Result", new CheckoutResultViewModel
        {
            Succeeded = result.Succeeded,
            OrderId = result.OrderId,
            Message = result.Message
        });
    }

    private async Task<CheckoutViewModel> BuildViewModelAsync(int userId)
    {
        var summary = await _checkoutService.GetCheckoutSummaryAsync(userId);

        return new CheckoutViewModel
        {
            Items = summary.Items.Select(item => new CheckoutItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList(),
            SavedAddresses = summary.SavedAddresses.Select(address => new CheckoutAddressViewModel
            {
                Id = address.Id,
                Title = address.Title,
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                City = address.City,
                District = address.District,
                AddressLine = address.AddressLine
            }).ToList(),
            SavedCards = summary.SavedCards.Select(card => new CheckoutCardViewModel
            {
                Id = card.Id,
                CardHolderName = card.CardHolderName,
                CardLastFour = card.CardLastFour
            }).ToList()
        };
    }

    private void ValidateAddress(CheckoutViewModel model)
    {
        if (model.SelectedAddressId.HasValue)
            return;

        AddRequiredAddressError(model.AddressTitle, nameof(model.AddressTitle), "Adres başlığı boş olamaz.");
        AddRequiredAddressError(model.FullName, nameof(model.FullName), "Ad soyad boş olamaz.");
        AddRequiredAddressError(model.PhoneNumber, nameof(model.PhoneNumber), "Telefon boş olamaz.");
        AddRequiredAddressError(model.City, nameof(model.City), "Şehir boş olamaz.");
        AddRequiredAddressError(model.District, nameof(model.District), "İlçe boş olamaz.");
        AddRequiredAddressError(model.AddressLine, nameof(model.AddressLine), "Adres boş olamaz.");
    }

    private void AddRequiredAddressError(string? value, string fieldName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            ModelState.AddModelError(fieldName, message);
    }

    private void ValidatePaymentCard(CheckoutViewModel model)
    {
        if (model.SelectedCardId.HasValue)
            return;

        AddRequiredAddressError(model.CardHolderName, nameof(model.CardHolderName), "Kart üzerindeki isim boş olamaz.");
        AddRequiredAddressError(model.CardNumber, nameof(model.CardNumber), "Kart numarası boş olamaz.");
        AddRequiredAddressError(model.ExpiryMonth, nameof(model.ExpiryMonth), "Ay boş olamaz.");
        AddRequiredAddressError(model.ExpiryYear, nameof(model.ExpiryYear), "Yıl boş olamaz.");
        AddRequiredAddressError(model.Cvv, nameof(model.Cvv), "CVV boş olamaz.");
    }
}

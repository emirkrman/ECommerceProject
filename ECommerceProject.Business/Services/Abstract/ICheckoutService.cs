using ECommerceProject.Business.Models.Checkout;

namespace ECommerceProject.Business.Services.Abstract;

public interface ICheckoutService
{
    Task<CheckoutSummaryModel> GetCheckoutSummaryAsync(int userId);
    Task<CheckoutResult> PlaceOrderAsync(int userId, CheckoutRequest request);
}

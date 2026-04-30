using ECommerceProject.Business.Models.Payments;
using ECommerceProject.Business.Services.Abstract;

namespace ECommerceProject.Business.Services.Concrete;

public class MockPaymentService : IPaymentService
{
    public Task<PaymentResult> PayAsync(PaymentRequest request)
    {
        var cardNumber = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        if (request.Amount <= 0)
            return Task.FromResult(PaymentResult.Failure("Ödeme tutarı geçersiz."));

        if (cardNumber.Length != 16)
            return Task.FromResult(PaymentResult.Failure("Kart numarası geçersiz."));

        if (cardNumber.EndsWith("0000", StringComparison.Ordinal))
            return Task.FromResult(PaymentResult.Failure("Mock ödeme reddedildi."));

        if (request.Cvv == "000")
            return Task.FromResult(PaymentResult.Failure("Mock güvenlik doğrulaması başarısız."));

        return Task.FromResult(PaymentResult.Success());
    }
}

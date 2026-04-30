using ECommerceProject.Business.Models.Payments;

namespace ECommerceProject.Business.Services.Abstract;

public interface IPaymentService
{
    Task<PaymentResult> PayAsync(PaymentRequest request);
}

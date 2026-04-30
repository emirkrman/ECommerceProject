namespace ECommerceProject.Business.Models.Payments;

public class PaymentRequest
{
    public decimal Amount { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public string ExpiryMonth { get; set; } = null!;

    public string ExpiryYear { get; set; } = null!;

    public string Cvv { get; set; } = null!;
}

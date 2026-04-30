namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutCardModel
{
    public int Id { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public string ExpiryMonth { get; set; } = null!;

    public string ExpiryYear { get; set; } = null!;

    public string Cvv { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;
}

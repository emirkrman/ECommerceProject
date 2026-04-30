namespace ECommerceProject.Web.ViewModels.Checkout;

public class CheckoutCardViewModel
{
    public int Id { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public string ExpiryMonth { get; set; } = null!;

    public string ExpiryYear { get; set; } = null!;

    public string Cvv { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;
}

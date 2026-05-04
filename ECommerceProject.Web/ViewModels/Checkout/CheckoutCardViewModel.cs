namespace ECommerceProject.Web.ViewModels.Checkout;

public class CheckoutCardViewModel
{
    public int Id { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;
}

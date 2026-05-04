namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutCardModel
{
    public int Id { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;
}

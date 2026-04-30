namespace ECommerceProject.Web.ViewModels.Checkout;

public class CheckoutResultViewModel
{
    public bool Succeeded { get; set; }

    public int? OrderId { get; set; }

    public string Message { get; set; } = null!;
}

namespace ECommerceProject.Web.ViewModels.Checkout;

public class CheckoutItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal => UnitPrice * Quantity;
}

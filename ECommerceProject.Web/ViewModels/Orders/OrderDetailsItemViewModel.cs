namespace ECommerceProject.Web.ViewModels.Orders;

public class OrderDetailsItemViewModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal => UnitPrice * Quantity;
}

using ECommerceProject.Entity.Common;

namespace ECommerceProject.Web.ViewModels.Orders;

public class OrderDetailsViewModel
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMessage { get; set; }

    public string? PaymentCardLastFour { get; set; }

    public OrderAddressViewModel? Address { get; set; }

    public List<OrderDetailsItemViewModel> Items { get; set; } = new();
}

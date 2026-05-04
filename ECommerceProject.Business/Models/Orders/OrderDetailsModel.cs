using ECommerceProject.Entity.Common;

namespace ECommerceProject.Business.Models.Orders;

public class OrderDetailsModel
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentMessage { get; set; }

    public string? PaymentCardLastFour { get; set; }

    public OrderAddressModel? Address { get; set; }

    public List<OrderDetailsItemModel> Items { get; set; } = new();
}

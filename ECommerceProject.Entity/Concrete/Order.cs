using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class Order : BaseEntity
{
    public int UserId { get; set; }

    public AppUser? User { get; set; }

    public int UserAddressId { get; set; }

    public UserAddress? UserAddress { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public string? PaymentMessage { get; set; }

    public string? PaymentCardLastFour { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

using ECommerceProject.Entity.Common;

namespace ECommerceProject.Business.Models.Orders;

public class OrderListItemModel
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public int ItemCount { get; set; }
}

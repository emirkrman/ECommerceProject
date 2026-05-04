using ECommerceProject.Entity.Common;

namespace ECommerceProject.Web.ViewModels.Orders;

public class OrderListItemViewModel
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public int ItemCount { get; set; }
}

using ECommerceProject.Business.Models.Orders;

namespace ECommerceProject.Business.Services.Abstract;

public interface IOrderService
{
    Task<List<OrderListItemModel>> GetUserOrdersAsync(int userId);

    Task<OrderDetailsModel?> GetUserOrderDetailsAsync(int userId, int orderId);
}

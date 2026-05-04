using ECommerceProject.Business.Models.Orders;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Business.Services.Concrete;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderListItemModel>> GetUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedDate)
            .Select(order => new OrderListItemModel
            {
                Id = order.Id,
                CreatedDate = order.CreatedDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ItemCount = order.Items.Sum(item => item.Quantity)
            })
            .ToListAsync();
    }

    public async Task<OrderDetailsModel?> GetUserOrderDetailsAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.UserAddress)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : MapDetails(order);
    }

    private static OrderDetailsModel MapDetails(Order order)
    {
        return new OrderDetailsModel
        {
            Id = order.Id,
            CreatedDate = order.CreatedDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMessage = order.PaymentMessage,
            PaymentCardLastFour = order.PaymentCardLastFour,
            Address = order.UserAddress == null ? null : new OrderAddressModel
            {
                Title = order.UserAddress.Title,
                FullName = order.UserAddress.FullName,
                PhoneNumber = order.UserAddress.PhoneNumber,
                City = order.UserAddress.City,
                District = order.UserAddress.District,
                AddressLine = order.UserAddress.AddressLine
            },
            Items = order.Items
                .OrderBy(item => item.Id)
                .Select(item => new OrderDetailsItemModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}

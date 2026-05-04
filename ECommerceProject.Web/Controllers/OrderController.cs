using ECommerceProject.Business.Models.Orders;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Web.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceProject.Web.Controllers;

[Authorize]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(
        IOrderService orderService,
        INavigationService navigationService)
        : base(navigationService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var orders = await _orderService.GetUserOrdersAsync(userId.Value);

        return View(new OrderHistoryViewModel
        {
            Orders = orders.Select(MapListItem).ToList()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Challenge();

        var order = await _orderService.GetUserOrderDetailsAsync(userId.Value, id);

        return order == null ? NotFound() : View(MapDetails(order));
    }

    private static OrderListItemViewModel MapListItem(OrderListItemModel order)
    {
        return new OrderListItemViewModel
        {
            Id = order.Id,
            CreatedDate = order.CreatedDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ItemCount = order.ItemCount
        };
    }

    private static OrderDetailsViewModel MapDetails(OrderDetailsModel order)
    {
        return new OrderDetailsViewModel
        {
            Id = order.Id,
            CreatedDate = order.CreatedDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMessage = order.PaymentMessage,
            PaymentCardLastFour = order.PaymentCardLastFour,
            Address = order.Address == null ? null : new OrderAddressViewModel
            {
                Title = order.Address.Title,
                FullName = order.Address.FullName,
                PhoneNumber = order.Address.PhoneNumber,
                City = order.Address.City,
                District = order.Address.District,
                AddressLine = order.Address.AddressLine
            },
            Items = order.Items.Select(item => new OrderDetailsItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}

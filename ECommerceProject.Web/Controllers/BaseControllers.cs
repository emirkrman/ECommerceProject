using System.Security.Claims;
using ECommerceProject.Business.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceProject.Web.Controllers;

public class BaseController : Controller
{
    private readonly INavigationService _navigationService;

    public BaseController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.NavCategories = await _navigationService.GetNavigationCategoriesAsync();

        await next();
    }

    protected int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}

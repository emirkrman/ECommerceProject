using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Common;
using ECommerceProject.Web.ViewModels.Common;
using ECommerceProject.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ECommerceProject.Web.Controllers;

public class HomeController : BaseController
{
    private readonly IProductService _productService;

    public HomeController(IProductService productService, INavigationService navigationService)
        : base(navigationService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _productService.GetHomeProductsAsync(page);

        return View(new HomeIndexViewModel
        {
            Products = result.Products,
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Admin()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}

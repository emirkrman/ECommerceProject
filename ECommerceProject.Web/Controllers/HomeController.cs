using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Common;
using ECommerceProject.Web.ViewModels.Common;
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

    public async Task<IActionResult> Index()
    {
        return View(await _productService.GetLatestActiveProductsAsync(12));
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

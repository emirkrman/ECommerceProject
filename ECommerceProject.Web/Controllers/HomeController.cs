using ECommerceProject.Data.Context;
using ECommerceProject.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Web.Controllers;

public class HomeController : BaseController
{
    public HomeController(AppDbContext context) : base(context) { }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => Guid.NewGuid())
            .Take(12)
            .ToListAsync();

        return View(products);
    }

    public IActionResult Admin()
    {
        return View();
    }
}
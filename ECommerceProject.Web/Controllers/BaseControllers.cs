using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Web.Controllers;

public class BaseController : Controller
{
    protected readonly AppDbContext _context;

    public BaseController(AppDbContext context)
    {
        _context = context;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.NavCategories = await GetNavigationCategoriesAsync();

        await next();
    }

    protected async Task<List<Category>> GetNavigationCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .ToListAsync();
    }
}

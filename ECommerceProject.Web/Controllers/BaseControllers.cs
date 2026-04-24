using ECommerceProject.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BaseController : Controller
{
    protected readonly AppDbContext _context;

    public BaseController(AppDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        var categories = _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .ToList();

        ViewBag.NavCategories = categories;

        base.OnActionExecuting(context);
    }
}
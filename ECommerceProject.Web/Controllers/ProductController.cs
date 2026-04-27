using ECommerceProject.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceProject.Entity.Concrete;
using ECommerceProject.Web.ViewModels.Products;

namespace ECommerceProject.Web.Controllers;

public class ProductController : BaseController
{
    public ProductController(AppDbContext context) : base(context) { }

    private static decimal GenerateInitialRating()
    {
        return Random.Shared.Next(35, 51) / 10m;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .ThenInclude(c => c.ParentCategory)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            "Id",
            "Name"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!categories.Any())
        {
            TempData["Error"] = "Ürün eklemeden önce kategori oluşturmalısınız.";
            return RedirectToAction("Create", "Category");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model.CategoryId
            );

            return View(model);
        }

        model.CreatedDate = DateTime.UtcNow;
        model.Rating = GenerateInitialRating();

        _context.Products.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            "Id",
            "Name",
            product.CategoryId
        );

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id)
            return NotFound();

        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!categories.Any())
        {
            TempData["Error"] = "Önce kategori oluşturmalısınız.";
            return RedirectToAction("Create", "Category");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model.CategoryId
            );

            return View(model);
        }

        var entity = await _context.Products.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Price = model.Price;
        entity.Stock = model.Stock;
        entity.ImageUrl = model.ImageUrl;
        entity.IsActive = model.IsActive;
        entity.CategoryId = model.CategoryId;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .ThenInclude(c => c.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> List(int? categoryId, string? search, string? sort = null, int page = 1)
    {
        int pageSize = 8;

        var query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            var subCategoryIds = await _context.Categories
                .Where(c => c.ParentCategoryId == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            subCategoryIds.Add(categoryId.Value);

            query = query.Where(p => subCategoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));
        }

        query = sort switch
        {
            "newest" => query.OrderByDescending(p => p.CreatedDate),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "rating_desc" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };

        var totalProducts = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .ToListAsync();

        var model = new ProductListViewModel
        {
            ListedProducts = products,
            Categories = categories,
            CategoryId = categoryId,
            Search = search,
            Sort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
        };

        return View(model);
    }
}

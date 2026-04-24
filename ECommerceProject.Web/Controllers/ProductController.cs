using ECommerceProject.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Web.Controllers;

public class ProductController : BaseController
{
    public ProductController(AppDbContext context) : base(context) { }

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
}
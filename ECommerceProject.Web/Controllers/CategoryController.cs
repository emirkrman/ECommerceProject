using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CategoryController : BaseController
{
    public CategoryController(AppDbContext context) : base(context) { }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    public async Task<IActionResult> Create()
    {
        await SetParentCategoriesViewBagAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid)
        {
            await SetParentCategoriesViewBagAsync(model.ParentCategoryId);
            return View(model);
        }

        model.CreatedDate = DateTime.UtcNow;

        _context.Categories.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _context.Categories.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.IsActive = model.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task SetParentCategoriesViewBagAsync(int? selectedParentCategoryId = null)
    {
        var parentCategories = await _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();

        ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name", selectedParentCategoryId);
    }
}

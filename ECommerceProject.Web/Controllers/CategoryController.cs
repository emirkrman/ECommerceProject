using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceProject.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService, INavigationService navigationService)
        : base(navigationService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _categoryService.GetListAsync());
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

        await _categoryService.CreateAsync(model);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
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

        return await _categoryService.UpdateAsync(id, model)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        return await _categoryService.DeleteAsync(id)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    private async Task SetParentCategoriesViewBagAsync(int? selectedParentCategoryId = null)
    {
        var parentCategories = await _categoryService.GetParentCategoriesAsync();
        ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name", selectedParentCategoryId);
    }
}

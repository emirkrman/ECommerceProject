using ECommerceProject.Business.Models.Products;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using ECommerceProject.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceProject.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ProductController : BaseController
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService, INavigationService navigationService)
        : base(navigationService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> List(int? categoryId, string? search, string? sort = null, int page = 1)
    {
        var result = await _productService.GetPublicListAsync(categoryId, search, sort, page);

        var model = new ProductListViewModel
        {
            ListedProducts = result.Products,
            Categories = result.Categories,
            CategoryId = result.CategoryId,
            Search = result.Search,
            Sort = result.Sort,
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetActiveDetailsAsync(id);
        return product == null ? NotFound() : View(product);
    }

    public async Task<IActionResult> Index(string? sort = null)
    {
        ViewBag.CurrentSort = sort;
        return View(await _productService.GetAdminListAsync(sort));
    }

    public async Task<IActionResult> Create()
    {
        await SetCategoriesViewBagAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        var categories = await _productService.GetActiveCategoriesAsync();
        if (!categories.Any())
        {
            TempData["Error"] = "Ürün eklemeden önce kategori oluşturmalısınız.";
            return RedirectToAction(nameof(CategoryController.Create), "Category");
        }

        AddImageValidationErrors(model.ImageFile);
        if (!ModelState.IsValid)
        {
            SetCategoriesViewBag(categories, model.CategoryId);
            return View(model);
        }

        await _productService.CreateAsync(MapToFormData(model));

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formData = await _productService.GetEditFormAsync(id);
        if (formData == null)
            return NotFound();

        await SetCategoriesViewBagAsync(formData.CategoryId);
        return View(MapToViewModel(formData));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var categories = await _productService.GetActiveCategoriesAsync();
        if (!categories.Any())
        {
            TempData["Error"] = "Önce kategori oluşturmalısınız.";
            return RedirectToAction(nameof(CategoryController.Create), "Category");
        }

        AddImageValidationErrors(model.ImageFile);
        if (!ModelState.IsValid)
        {
            SetCategoriesViewBag(categories, model.CategoryId);
            return View(model);
        }

        return await _productService.UpdateAsync(id, MapToFormData(model))
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productService.GetForDeleteAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        return await _productService.DeleteAsync(id)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    private async Task SetCategoriesViewBagAsync(int? selectedCategoryId = null)
    {
        SetCategoriesViewBag(await _productService.GetActiveCategoriesAsync(), selectedCategoryId);
    }

    private void SetCategoriesViewBag(IEnumerable<Category> categories, int? selectedCategoryId = null)
    {
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }

    private void AddImageValidationErrors(IFormFile? imageFile)
    {
        foreach (var error in _productService.ValidateImage(imageFile, nameof(ProductFormViewModel.ImageFile)))
        {
            ModelState.AddModelError(error.FieldName, error.Message);
        }
    }

    private static ProductFormData MapToFormData(ProductFormViewModel model)
    {
        return new ProductFormData
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Stock = model.Stock,
            ExistingImageUrl = model.ExistingImageUrl,
            ImageFile = model.ImageFile,
            IsActive = model.IsActive,
            CategoryId = model.CategoryId
        };
    }

    private static ProductFormViewModel MapToViewModel(ProductFormData formData)
    {
        return new ProductFormViewModel
        {
            Id = formData.Id,
            Name = formData.Name,
            Description = formData.Description,
            Price = formData.Price,
            Stock = formData.Stock,
            ExistingImageUrl = formData.ExistingImageUrl,
            IsActive = formData.IsActive,
            CategoryId = formData.CategoryId
        };
    }
}

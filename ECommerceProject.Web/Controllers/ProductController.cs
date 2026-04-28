using AutoMapper;
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
    private readonly IMapper _mapper;

    public ProductController(
        IProductService productService,
        INavigationService navigationService,
        IMapper mapper)
        : base(navigationService)
    {
        _productService = productService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    public async Task<IActionResult> List(int? categoryId, string? search, string? sort = null, int page = 1)
    {
        var result = await _productService.GetPublicListAsync(categoryId, search, sort, page);
        var model = _mapper.Map<ProductListViewModel>(result);

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

        await _productService.CreateAsync(_mapper.Map<ProductFormData>(model));

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formData = await _productService.GetEditFormAsync(id);
        if (formData == null)
            return NotFound();

        await SetCategoriesViewBagAsync(formData.CategoryId);
        return View(_mapper.Map<ProductFormViewModel>(formData));
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

        return await _productService.UpdateAsync(id, _mapper.Map<ProductFormData>(model))
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

}

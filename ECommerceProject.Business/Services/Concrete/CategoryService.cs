using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Data.UnitOfWork;
using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Concrete;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Category>> GetListAsync()
    {
        return await _categoryRepository.GetOrderedAsync();
    }

    public async Task<List<Category>> GetParentCategoriesAsync()
    {
        return await _categoryRepository.GetParentCategoriesAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    public async Task CreateAsync(Category model)
    {
        model.CreatedDate = DateTime.UtcNow;

        await _categoryRepository.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int id, Category model)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity == null)
            return false;

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.IsActive = model.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return false;

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

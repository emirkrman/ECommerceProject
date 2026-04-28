using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Abstract;

public interface ICategoryService
{
    Task<List<Category>> GetListAsync();
    Task<List<Category>> GetParentCategoriesAsync();
    Task<Category?> GetByIdAsync(int id);
    Task CreateAsync(Category model);
    Task<bool> UpdateAsync(int id, Category model);
    Task<bool> DeleteAsync(int id);
}

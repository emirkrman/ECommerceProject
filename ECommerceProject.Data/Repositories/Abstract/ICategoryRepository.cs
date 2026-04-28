using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Data.Repositories.Abstract;

public interface ICategoryRepository
{
    Task<List<Category>> GetOrderedAsync();
    Task<List<Category>> GetActiveAsync();
    Task<List<Category>> GetParentCategoriesAsync();
    Task<List<Category>> GetNavigationCategoriesAsync();
    Task<List<int>> GetCategoryAndSubCategoryIdsAsync(int categoryId);
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    void Remove(Category category);
    Task SaveChangesAsync();
}

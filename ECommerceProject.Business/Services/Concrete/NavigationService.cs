using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Concrete;

public class NavigationService : INavigationService
{
    private readonly ICategoryRepository _categoryRepository;

    public NavigationService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> GetNavigationCategoriesAsync()
    {
        return await _categoryRepository.GetNavigationCategoriesAsync();
    }
}

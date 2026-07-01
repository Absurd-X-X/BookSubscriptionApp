using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);
        Task<Category?> GetCategoryAsync(Guid id);
        Task<bool> IsExistAsync(string name);
        Task<ICollection<Category>> GetAllCategoriesAsync();

        Task<PagenatedList<Category>> GetCategoriesAsync(bool usepaging, PageRequest request);
        Task<ICollection<Category>> GetByNameAsync(string name);
    }
}

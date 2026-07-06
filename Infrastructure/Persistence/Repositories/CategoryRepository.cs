using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Infrastructure.Persistence.Repositories
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        public async Task AddAsync(Category category)

            => await context.Categories.AddAsync(category);

        public async Task<ICollection<Category>> GetAllCategoriesAsync()

            => await context.Categories
            .Include(x => x.Books)
            .Where(v => !v.IsDeleted)
            .ToListAsync();

        public async Task<Category?> GetCategoryAsync(Guid id)

            => await context.Categories
            .Include(x => x.Books)
            .FirstOrDefaultAsync(v => !v.IsDeleted && v.Id == id);


        public async Task<ICollection<Category>> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return [];

            string search = name.Trim().ToLower();
            return await context.Categories.Where(a => (a.Name.Contains(search) || a.Description.Contains(search)) && !a.IsDeleted).ToListAsync();
        }

        public async Task<bool> IsExistAsync(string name)

            => await context.Categories
                .AnyAsync(x => x.Name == name);

        public async Task<PagenatedList<Category>> GetCategoriesAsync(bool usePaging, PageRequest request)
        {
            var query = context.Categories.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Books)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Category>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Category>
            {
                Items = await query.Include(x => x.Books).ToListAsync(),
                TotalCount = totalCount
            };
        }
    }
}

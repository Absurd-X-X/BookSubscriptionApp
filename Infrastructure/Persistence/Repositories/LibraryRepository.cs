using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class LibraryRepository(AppDbContext context) : ILibraryRepository
    {
        public async Task AddAsync(Library library)

            => await context.Libraries.AddAsync(library);

        public async Task<PagenatedList<Library>> GetAllAsync(PageRequest request, bool usePaging)
        {
            var query = context.Libraries.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Books)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Library>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Library>
            {
                Items = await query.Include(x => x.Books).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<Library?> GetAsync(Guid id)

            => await context.Libraries
            .Include(n => n.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Library?> GetLibraryAsync(string email)
        
            => await context.Libraries
                    .FirstOrDefaultAsync(x => x.Email == email);
    }
}

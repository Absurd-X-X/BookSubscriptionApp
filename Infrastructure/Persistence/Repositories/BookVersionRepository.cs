using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BookVersionRepository(AppDbContext context) : IBookVersionRepository
    {
        public async Task AddAsync(BookVersion version)
            => await context.BookVersions.AddAsync(version);

        public async Task<BookVersion?> GetCurrentAsync(Guid bookId)
            => await context.BookVersions
                .FirstOrDefaultAsync(v => v.BookId == bookId && v.IsCurrent);

        public async Task<PagenatedList<BookVersion>> GetByBookIdAsync(Guid bookId, PageRequest request, bool usePaging)
        {
            var query = context.BookVersions
                .Where(v => v.BookId == bookId)
                .OrderByDescending(v => v.UploadedAt)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var items = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagenatedList<BookVersion>
                {
                    Items = items,
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<BookVersion>
            {
                Items = await query.ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<BookVersion?> GetByIdAsync(Guid id)
            => await context.BookVersions.FirstOrDefaultAsync(v => v.Id == id);

        public async Task SetCurrentAsync(Guid bookId, Guid versionId)
        {
            var versions = await context.BookVersions
                .Where(v => v.BookId == bookId)
                .ToListAsync();

            foreach (var v in versions)
            {
                v.IsCurrent = v.Id == versionId;
            }
        }

        public void Update(BookVersion version)
            => context.Update(version);
    }
}
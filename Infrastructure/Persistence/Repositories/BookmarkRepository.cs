using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class BookmarkRepository(AppDbContext context)
        : IBookmarkRepository
    {
        public async Task AddAsync(Bookmark bookmark)
        {
            await context.Bookmarks.AddAsync(bookmark);
        }

        public async Task RemoveAsync(Bookmark bookmark)
        {
            context.Bookmarks.Remove(bookmark);
        }

        public async Task<Bookmark?> GetByIdAsync(Guid id)
        {
            return await context.Bookmarks
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Bookmark>> GetReaderBookmarksAsync(Guid readerId)
        {
            return await context.Bookmarks
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId)
                .ToListAsync();
        }

        public async Task<PagenatedList<Bookmark>> GetReaderBookmarksPagedAsync(
            Guid readerId,
            PageRequest page,
            string? search,
            string? sortBy)
        {
            var query = context.Bookmarks
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId && !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Book.Title.ToLower().Contains(term) ||
                    x.Quote.ToLower().Contains(term) ||
                    x.Note.ToLower().Contains(term));
            }

            query = sortBy switch
            {
                "title" => query.OrderBy(x => x.Book.Title),
                "page" => query.OrderBy(x => x.PageNumber),
                _ => query.OrderByDescending(x => x.DateCreated)
            };

            var totalCount = await query.CountAsync();

            int currentPage = page.Page < 1 ? 1 : page.Page;
            int pageSize = page.PageSize < 1 ? 12 : page.PageSize;

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagenatedList<Bookmark>
            {
                Items = items,
                Page = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
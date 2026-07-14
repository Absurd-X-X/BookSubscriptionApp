using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class FavoriteRepository(AppDbContext context)
        : IFavoriteRepository
    {
        public async Task AddAsync(Favorite favorite)
        {
            await context.Favorites.AddAsync(favorite);
        }

        public async Task RemoveAsync(Favorite favorite)
        {
            context.Favorites.Remove(favorite);
            await Task.CompletedTask;
        }

        public async Task<Favorite?> GetAsync(Guid readerId, Guid bookId)
        {
            return await context.Favorites
                .FirstOrDefaultAsync(x =>
                    x.ReaderId == readerId &&
                    x.BookId == bookId);
        }

        public async Task<bool> IsFavoriteAsync(Guid readerId, Guid bookId)
        {
            return await context.Favorites
                .AnyAsync(x =>
                    x.ReaderId == readerId &&
                    x.BookId == bookId);
        }

        public async Task<List<Book>> GetReaderFavoritesAsync(Guid readerId)
        {
            return await context.Favorites
                .Where(x => x.ReaderId == readerId)
                .Select(x => x.Book)
                .ToListAsync();
        }

        public async Task<int> GetBookFavoriteCountAsync(Guid bookId)
        {
            return await context.Favorites
                .CountAsync(x => x.BookId == bookId);
        }

        public async Task<PagenatedList<Favorite>> GetReaderFavoritesPagedAsync(
            Guid readerId,
            PageRequest page,
            string? search,
            Guid? categoryId,
            string? sortBy)
        {
            var query = context.Favorites
                .Include(x => x.Book)
                .ThenInclude(b => b.Category)
                .Where(x => x.ReaderId == readerId && !x.Book.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x =>
                    x.Book.Title.ToLower().Contains(term) ||
                    x.Book.Author.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.Book.Category.Id == categoryId.Value);
            }

            query = sortBy switch
            {
                "title" => query.OrderBy(x => x.Book.Title),
                "author" => query.OrderBy(x => x.Book.Author),
                _ => query.OrderByDescending(x => x.CreatedAt) 
            };

            var totalCount = await query.CountAsync();

            int currentPage = page.Page < 1 ? 1 : page.Page;
            int pageSize = page.PageSize < 1 ? 12 : page.PageSize;

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagenatedList<Favorite>
            {
                Items = items,
                Page = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
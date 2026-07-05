using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Persistence.Repositories
{
    public class ReviewRepository(AppDbContext context) : IReviewRepository
    {
        public async Task AddAsync(Review review)
        {
            await context.Reviews.AddAsync(review);
        }

        public async Task<int> CountByBookIdAsync(Guid bookId)
        {
            return  context.Reviews.Count(x => x.BookId == bookId);
        }

        public async Task<PagenatedList<Review>> GetAllAsync(PageRequest request, bool usePaging)
        {
            var query = context.Reviews.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.DateCreated)
                    .Include(x => x.Reader)
                    .Include(x => x.Book)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Review>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Review>
            {
                Items = await query.Include(x => x.Reader).Include(x => x.Book).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<Review>> GetByBookIdAsync(PageRequest request, bool usePaging, Guid bookId)
        {
            var query = context.Reviews.Where(x => !x.IsDeleted && x.BookId == bookId)
                .AsQueryable();

            var totalCount = query.Count();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.DateCreated)
                    .Include(x => x.Reader)
                    .Include(x => x.Book)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Review>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Review>
            {
                Items = await query.Include(x => x.Reader).Include(x => x.Book).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<Review?> GetByIdAsync(Guid reviewId)
        {
            return await context.Reviews
                .Include(x => x.Reader)
                .Include(q => q.Book).FirstOrDefaultAsync(c => c.Id == reviewId);
        }

        public async Task<PagenatedList<Review>> GetByLibraryIdAsync(PageRequest request, bool usePaging, Guid libraryId)
        {
            var query = context.Reviews.Where(x => !x.IsDeleted && x.Book.LibraryId == libraryId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.DateCreated)
                    .Include(x => x.Reader)
                    .Include(x => x.Book)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Review>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Review>
            {
                Items = await query.Include(x => x.Reader).Include(x => x.Book).ToListAsync(),
                TotalCount = totalCount
            };
        }
    }
}

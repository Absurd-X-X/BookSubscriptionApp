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

        public async Task<int> CountByReaderIdAsync(Guid readerId)
        {
            return context.Reviews.Count(x => x.ReaderId == readerId);
        }

        public async Task<double> GetAverageRatingForBookAsync(Guid bookId)
        {
            var ratings = await context.Reviews
                .Where(x => !x.IsDeleted && x.BookId == bookId)
                .Select(x => x.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
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

        public async Task<List<Review>> GetByLibraryIdAndDateRangeAsync(Guid libraryId, DateTime start, DateTime end)
        {
            return await context.Reviews
                .Where(x => !x.IsDeleted
                    && x.Book.LibraryId == libraryId
                    && x.DateCreated >= start
                    && x.DateCreated <= end)
                .Include(x => x.Reader)
                .Include(x => x.Book)
                .ToListAsync();
        }

        // ReviewRepository.cs — add these methods
        public async Task<List<Review>> GetByReaderIdAsync(Guid readerId, int take)
        {
            return await context.Reviews
                .Where(x => !x.IsDeleted && x.ReaderId == readerId)
                .Include(x => x.Book)
                .OrderByDescending(x => x.DateCreated)
                .Take(take)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingGivenByReaderAsync(Guid readerId)
        {
            var ratings = await context.Reviews
                .Where(x => !x.IsDeleted && x.ReaderId == readerId)
                .Select(x => x.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionByReaderAsync(Guid readerId)
        {
            var ratings = await context.Reviews
                .Where(x => !x.IsDeleted && x.ReaderId == readerId)
                .GroupBy(x => x.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var dist = Enumerable.Range(1, 5).ToDictionary(r => r, r => 0);
            foreach (var r in ratings)
            {
                if (dist.ContainsKey(r.Rating)) dist[r.Rating] = r.Count;
            }
            return dist;
        }

        // ReviewRepository.cs — add this
        public async Task<double> GetAverageRatingGivenByReaderInYearAsync(Guid readerId, int year)
        {
            var ratings = await context.Reviews
                .Where(x => !x.IsDeleted &&
                            x.ReaderId == readerId &&
                            x.DateCreated.Year == year)
                .Select(x => x.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<PagenatedList<Review>> GetPagedByReaderIdAsync(
            Guid readerId, PageRequest request, bool usePaging,
            string? search = null, string? sortBy = null,
            int? ratingFilter = null, Guid? bookIdFilter = null)
        {
            var query = context.Reviews
                .Where(x => !x.IsDeleted && x.ReaderId == readerId)
                .Include(x => x.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.Book.Title.Contains(s) ||
                    x.Book.Author.Contains(s) ||
                    x.Comment.Contains(s));
            }

            if (ratingFilter.HasValue)
                query = query.Where(x => x.Rating == ratingFilter.Value);

            if (bookIdFilter.HasValue)
                query = query.Where(x => x.BookId == bookIdFilter.Value);

            query = sortBy switch
            {
                "oldest" => query.OrderBy(x => x.DateCreated),
                "rating-high" => query.OrderByDescending(x => x.Rating),
                "rating-low" => query.OrderBy(x => x.Rating),
                "most-liked" => query.OrderByDescending(x => x.HelpfulCount),
                _ => query.OrderByDescending(x => x.DateCreated) // "newest" / default
            };

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 6 : request.PageSize;

                var items = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagenatedList<Review>
                {
                    Items = items,
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Review>
            {
                Items = await query.ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<List<(Guid BookId, string Title)>> GetReviewedBookOptionsAsync(Guid readerId)
        {
            return await context.Reviews
                .Where(x => !x.IsDeleted && x.ReaderId == readerId)
                .Select(x => new { x.BookId, x.Book.Title })
                .Distinct()
                .OrderBy(x => x.Title)
                .Select(x => new ValueTuple<Guid, string>(x.BookId, x.Title))
                .ToListAsync();
        }
    }
}

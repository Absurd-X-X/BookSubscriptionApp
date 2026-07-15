// Application.Queries/GetTopPerformingBooks.cs
using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetTopPerformingBooks
    {
        public record GetTopPerformingBooksQuery(Guid LibraryId, int Page, int PageSize)
            : IRequest<Result<PagenatedList<TopPerformingBookDto>>>;

        public class GetTopPerformingBooksHandler(
            IBookRepository bookRepository,
            IReviewRepository reviewRepository,
            IReadingProgressRepository readingProgressRepository
            ) : IRequestHandler<GetTopPerformingBooksQuery, Result<PagenatedList<TopPerformingBookDto>>>
        {
            public async Task<Result<PagenatedList<TopPerformingBookDto>>> Handle(
                GetTopPerformingBooksQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetByLibraryIdAsync(request.LibraryId, new PageRequest
                {
                    Page = 1,
                    PageSize = int.MaxValue
                }, false);

                var allProgress = await readingProgressRepository.GetByLibraryIdAsync(
                    request.LibraryId, DateTime.MinValue, DateTime.MaxValue);

                var allReviews = await reviewRepository.GetByLibraryIdAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = int.MaxValue
                }, false, request.LibraryId);

                var rows = books.Items
                    .OrderByDescending(b => b.NoOfTimeReadByPeople)
                    .Select(b =>
                    {
                        var bookProgress = allProgress.Where(p => p.BookId == b.Id).ToList();
                        var completion = bookProgress.Count > 0
                            ? (int)Math.Round((double)bookProgress.Count(p => p.IsCompleted) / bookProgress.Count * 100)
                            : 0;

                        var bookReviews = allReviews.Items.Where(r => r.BookId == b.Id).ToList();
                        var avgRating = bookReviews.Any() ? Math.Round(bookReviews.Average(r => r.Rating), 1) : 0;

                        return new TopPerformingBookDto(
                            BookId: b.Id,
                            Title: b.Title,
                            Author: b.Author,
                            CoverUrl: b.BookCoverUrl,
                            Reads: b.NoOfTimeReadByPeople,
                            AverageRating: avgRating,
                            ReviewCount: bookReviews.Count,
                            CompletionRate: completion,
                            AddedOn: b.DateCreated,
                            IsPublished: b.IsPublished
                        );
                    })
                    .ToList();

                var totalCount = rows.Count;
                var currentPage = request.Page < 1 ? 1 : request.Page;
                var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var paged = rows
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new PagenatedList<TopPerformingBookDto>
                {
                    Items = paged,
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return Result<PagenatedList<TopPerformingBookDto>>.Success(response, "Success");
            }
        }

        public record TopPerformingBookDto(
            Guid BookId,
            string Title,
            string Author,
            string? CoverUrl,
            int Reads,
            double AverageRating,
            int ReviewCount,
            int CompletionRate,
            DateTime AddedOn,
            bool IsPublished
        );
    }
}
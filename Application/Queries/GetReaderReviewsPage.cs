using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public sealed record GetReaderReviewsPageQuery(
        Guid ReaderId,
        int Page = 1,
        int PageSize = 6,
        string? Search = null,
        string? SortBy = null,
        int? RatingFilter = null,
        Guid? BookIdFilter = null)
        : IRequest<ReaderReviewsPageVm>;

    public sealed class ReaderReviewsPageVm
    {
        public List<ReviewListItemVm> Reviews { get; init; } = [];
        public List<BookFilterOptionVm> BookOptions { get; init; } = [];
        public int CurrentPage { get; init; }
        public int PageSize { get; init; }
        public long TotalCount { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    public sealed class BookFilterOptionVm
    {
        public Guid BookId { get; init; }
        public string Title { get; init; } = default!;
    }

    public sealed class ReviewListItemVm
    {
        public Guid ReviewId { get; init; }
        public Guid BookId { get; init; }
        public string BookTitle { get; init; } = default!;
        public string BookAuthor { get; init; } = default!;
        public string? BookCoverUrl { get; init; }
        public int Rating { get; init; }
        public string Comment { get; init; } = default!;
        public int HelpfulCount { get; init; }
        public DateTime DateCreated { get; init; }
    }

    public sealed class GetReaderReviewsPageQueryHandler(IReviewRepository reviewRepository)
        : IRequestHandler<GetReaderReviewsPageQuery, ReaderReviewsPageVm>
    {
        public async Task<ReaderReviewsPageVm> Handle(GetReaderReviewsPageQuery request, CancellationToken ct)
        {
            var result = await reviewRepository.GetPagedByReaderIdAsync(
                request.ReaderId,
                new PageRequest { Page = request.Page, PageSize = request.PageSize },
                usePaging: true,
                search: request.Search,
                sortBy: request.SortBy,
                ratingFilter: request.RatingFilter,
                bookIdFilter: request.BookIdFilter);

            var bookOptions = await reviewRepository.GetReviewedBookOptionsAsync(request.ReaderId);

            return new ReaderReviewsPageVm
            {
                Reviews = result.Items.Select(r => new ReviewListItemVm
                {
                    ReviewId = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    BookAuthor = r.Book.Author,
                    BookCoverUrl = r.Book.BookCoverUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    HelpfulCount = r.HelpfulCount,
                    DateCreated = r.DateCreated
                }).ToList(),
                BookOptions = bookOptions.Select(b => new BookFilterOptionVm { BookId = b.BookId, Title = b.Title }).ToList(),
                CurrentPage = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}
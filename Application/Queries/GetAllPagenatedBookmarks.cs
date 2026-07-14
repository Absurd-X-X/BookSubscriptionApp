using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllPagenatedBookmarks
    {
        public record GetAllPagenatedBookmarksQuery(
            Guid ReaderId,
            int Page,
            int PageSize,
            string? Search,
            string? SortBy
            ) : IRequest<Result<PagenatedList<GetAllPagenatedBookmarksResponse>>>;

        public class GetAllBookmarksHandler(
            IBookmarkRepository bookmarkRepository
            ) : IRequestHandler<GetAllPagenatedBookmarksQuery, Result<PagenatedList<GetAllPagenatedBookmarksResponse>>>
        {
            async Task<Result<PagenatedList<GetAllPagenatedBookmarksResponse>>> IRequestHandler<GetAllPagenatedBookmarksQuery, Result<PagenatedList<GetAllPagenatedBookmarksResponse>>>.
                Handle(GetAllPagenatedBookmarksQuery request, CancellationToken cancellationToken)
            {
                var page = new PageRequest { Page = request.Page, PageSize = request.PageSize };

                var bookmarks = await bookmarkRepository.GetReaderBookmarksPagedAsync(
                    request.ReaderId, page, request.Search, request.SortBy);

                var data = bookmarks.Items.Select(x => new GetAllPagenatedBookmarksResponse(
                    x.Id,
                    x.Book.Id,
                    x.Book.Title,
                    x.Book.Author,
                    x.Book.BookCoverUrl,
                    x.PageNumber,
                    x.Quote,
                    x.Note,
                    x.DateCreated
                    )).ToList();

                var pagedData = new PagenatedList<GetAllPagenatedBookmarksResponse>
                {
                    Items = data,
                    Page = bookmarks.Page,
                    PageSize = bookmarks.PageSize,
                    TotalCount = bookmarks.TotalCount
                };

                return Result<PagenatedList<GetAllPagenatedBookmarksResponse>>.Success(pagedData, "Retrieved");
            }
        }

        public record GetAllPagenatedBookmarksResponse(
            Guid BookmarkId,
            Guid BookId,
            string BookTitle,
            string BookAuthor,
            string BookCoverUrl,
            int PageNumber,
            string Quote,
            string Note,
            DateTime DateAdded
            );
    }
}
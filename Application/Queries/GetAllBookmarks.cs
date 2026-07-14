using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllBookmarks
    {
        public record GetAllBookmarksQuery(
            Guid ReaderId
            ) : IRequest<Result<List<GetAllBookmarksResponse>>>;

        public class GetAllBookmarksHandler(
            IBookmarkRepository bookmarkRepository,
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetAllBookmarksQuery, Result<List<GetAllBookmarksResponse>>>
        {
            async Task<Result<List<GetAllBookmarksResponse>>> IRequestHandler<GetAllBookmarksQuery, Result<List<GetAllBookmarksResponse>>>.
                Handle(GetAllBookmarksQuery request, CancellationToken cancellationToken)
            {
                var bookmarks = await bookmarkRepository.GetReaderBookmarksAsync(request.ReaderId);

                var data = new List<GetAllBookmarksResponse>();

                foreach (var x in bookmarks.Where(x => !x.IsDeleted))
                {
                    var avgRating = await reviewRepository.GetAverageRatingForBookAsync(x.Book.Id);

                    data.Add(new GetAllBookmarksResponse(
                        x.Id,
                        x.Book.Id,
                        x.Book.Title,
                        x.Book.Author,
                        x.Book.BookCoverUrl,
                        avgRating,
                        x.PageNumber,
                        x.Quote,
                        x.Note,
                        x.DateCreated
                        ));
                }

                return Result<List<GetAllBookmarksResponse>>.Success(data, "Retrieved");
            }
        }

        public record GetAllBookmarksResponse(
            Guid BookmarkId,
            Guid BookId,
            string BookTitle,
            string BookAuthor,
            string BookCoverUrl,
            double AverageRating,
            int PageNumber,
            string Quote,
            string Note,
            DateTime DateAdded
            );
    }
}
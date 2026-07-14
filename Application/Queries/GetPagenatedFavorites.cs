using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetPagenatedFavorites
    {
        public record GetPagenatedFavoritesQuery(
            Guid ReaderId,
            int Page,
            int PageSize,
            string? Search,
            Guid? CategoryId,
            string? SortBy
            ) : IRequest<Result<PagenatedList<GetPagenatedFavoritesResponse>>>;

        public class GetPagenatedFavoritesHandler(
            IFavoriteRepository favoriteRepository,
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetPagenatedFavoritesQuery, Result<PagenatedList<GetPagenatedFavoritesResponse>>>
        {
            async Task<Result<PagenatedList<GetPagenatedFavoritesResponse>>> IRequestHandler<GetPagenatedFavoritesQuery, Result<PagenatedList<GetPagenatedFavoritesResponse>>>.
                Handle(GetPagenatedFavoritesQuery request, CancellationToken cancellationToken)
            {
                var page = new PageRequest { Page = request.Page, PageSize = request.PageSize };

                var favorites = await favoriteRepository.GetReaderFavoritesPagedAsync(
                    request.ReaderId, page, request.Search, request.CategoryId, request.SortBy);

                var data = new List<GetPagenatedFavoritesResponse>();

                foreach (var f in favorites.Items)
                {
                    var avgRating = await reviewRepository.GetAverageRatingForBookAsync(f.Book.Id);

                    data.Add(new GetPagenatedFavoritesResponse(
                        f.Book.Id,
                        f.Book.Title,
                        f.Book.Author,
                        f.Book.BookCoverUrl,
                        avgRating,
                        f.Book.Category.Id,
                        f.Book.Category.Name,
                        f.CreatedAt
                        ));
                }

                var pagedData = new PagenatedList<GetPagenatedFavoritesResponse>
                {
                    Items = data,
                    Page = favorites.Page,
                    PageSize = favorites.PageSize,
                    TotalCount = favorites.TotalCount
                };

                return Result<PagenatedList<GetPagenatedFavoritesResponse>>.Success(pagedData, "Retrieved");
            }
        }

        public record GetPagenatedFavoritesResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            double AverageRating,
            Guid CategoryId,
            string CategoryName,
            DateTime DateAdded
            );
    }
}
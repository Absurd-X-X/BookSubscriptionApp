using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetFavorites
    {
        public record GetFavoritesQuery(
            Guid ReaderId
            ) : IRequest<Result<List<GetFavoritesResponse>>>;

        public class GetFavoritesHandler(
            IFavoriteRepository favoriteRepository,
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetFavoritesQuery, Result<List<GetFavoritesResponse>>>
        {
            async Task<Result<List<GetFavoritesResponse>>> IRequestHandler<GetFavoritesQuery, Result<List<GetFavoritesResponse>>>.
                Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
            {
                var books = await favoriteRepository.GetReaderFavoritesAsync(request.ReaderId);

                var data = new List<GetFavoritesResponse>();

                foreach (var x in books)
                {
                    var avgRating = await reviewRepository.GetAverageRatingForBookAsync(x.Id);

                    data.Add(new GetFavoritesResponse(
                        x.Id,
                        x.Title,
                        x.Author,
                        x.BookCoverUrl,
                        avgRating
                        ));
                }

                return Result<List<GetFavoritesResponse>>.Success(data, "Retrieved");
            }
        }

        public record GetFavoritesResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            double AverageRating
            );
    }
}
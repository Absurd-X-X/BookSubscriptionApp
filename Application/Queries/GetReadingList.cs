using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetReadingList
    {
        public record GetReadingListQuery(
            Guid ReaderId
            ) : IRequest<Result<List<GetReadingListResponse>>>;

        public class GetReadingListHandler(
            IReadingListRepository readingListRepository,
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetReadingListQuery, Result<List<GetReadingListResponse>>>
        {
            async Task<Result<List<GetReadingListResponse>>> IRequestHandler<GetReadingListQuery, Result<List<GetReadingListResponse>>>.
                Handle(GetReadingListQuery request, CancellationToken cancellationToken)
            {
                var books = await readingListRepository.GetReaderReadingListAsync(request.ReaderId);

                var data = new List<GetReadingListResponse>();

                foreach (var x in books)
                {
                    var avgRating = await reviewRepository.GetAverageRatingForBookAsync(x.Id);

                    data.Add(new GetReadingListResponse(
                        x.Id,
                        x.Title,
                        x.Author,
                        x.BookCoverUrl,
                        avgRating
                        ));
                }

                return Result<List<GetReadingListResponse>>.Success(data, "Retrieved");
            }
        }

        public record GetReadingListResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            double AverageRating
            );
    }
}
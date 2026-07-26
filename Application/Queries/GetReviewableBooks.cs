using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetReviewableBooks
    {
        public record GetReviewableBooksQuery(Guid ReaderId)
            : IRequest<Result<List<GetReviewableBooksResponse>>>;

        public class GetReviewableBooksHandler(
            IReaderRepository readerRepository,
            IReadingProgressRepository readingProgressRepository,
            IReviewRepository reviewRepository)
            : IRequestHandler<GetReviewableBooksQuery, Result<List<GetReviewableBooksResponse>>>
        {
            public async Task<Result<List<GetReviewableBooksResponse>>> Handle(
                GetReviewableBooksQuery request,
                CancellationToken cancellationToken)
            {
                var reader = await readerRepository.GetByIdAsync(request.ReaderId);
                if (reader is null)
                {
                    return Result<List<GetReviewableBooksResponse>>.Failure("Reader profile not found.");
                }

                var progresses = await readingProgressRepository.GetByReaderAsync(request.ReaderId);

                if (!progresses.Any())
                {
                    return Result<List<GetReviewableBooksResponse>>
                        .Success(new List<GetReviewableBooksResponse>(), "Success");
                }

                var reviewedBookIds = (await reviewRepository
                        .GetReviewedBookOptionsAsync(request.ReaderId))
                    .Select(x => x.BookId)
                    .ToHashSet();

                var response = progresses
                    .Where(p => !p.IsDeleted && !reviewedBookIds.Contains(p.BookId))
                    .Select(p => new GetReviewableBooksResponse(
                        p.BookId,
                        p.Book.Title,
                        p.Book.Author))
                    .DistinctBy(x => x.BookId)
                    .OrderBy(x => x.Title)
                    .ToList();

                return Result<List<GetReviewableBooksResponse>>
                    .Success(response, "Success");
            }
        }

        public record GetReviewableBooksResponse(
            Guid BookId,
            string Title,
            string Author
        );
    }
}
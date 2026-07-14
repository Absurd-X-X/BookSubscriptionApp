using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetReadingProgress
    {
        public record GetReadingProgressQuery(Guid ReaderId, Guid BookId) : IRequest<Result<ReadingProgressResponse>>;

        public class GetReadingProgressHandler : IRequestHandler<GetReadingProgressQuery, Result<ReadingProgressResponse>>
        {
            private readonly IReadingProgressRepository _repository;
            public GetReadingProgressHandler(IReadingProgressRepository repository) => _repository = repository;

            public async Task<Result<ReadingProgressResponse>> Handle(GetReadingProgressQuery request, CancellationToken cancellationToken)
            {
                var progress = await _repository.GetAsync(request.ReaderId, request.BookId);

                if (progress == null)
                {
                    return Result<ReadingProgressResponse>.Failure("Reading progress not found.");
                }

                var response = new ReadingProgressResponse(
                    progress.ReaderId,
                    progress.BookId,
                    progress.ProgressPercentage,
                    progress.CurrentChapter,
                    progress.CurrentPage,
                    progress.LastReadDate.HasValue ? progress.LastReadDate.Value : DateTime.MinValue,
                    progress.IsCompleted
                );

                return Result<ReadingProgressResponse>.Success(response, "Reading progress retrieved successfully.");
            }
        }

        public record ReadingProgressResponse(
            Guid ReaderId,
            Guid BookId,
            double Percentage,
            string? CurrentChapter,
            int CurrentPage,
            DateTime LastReadAt,
            bool IsCompleted
        );
    }
}

using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetBookForReading
    {
        public record GetBookForReadingQuery(
            Guid ReaderId,
            Guid BookId)
            : IRequest<Result<GetBookForReadingResponse>>;

        public class GetBookForReadingHandler(
    IBookRepository bookRepository,IUnitOfWork unitOfWork,
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository,
    IReadingProgressRepository readingProgressRepository)
    : IRequestHandler<GetBookForReadingQuery,
        Result<GetBookForReadingResponse>>
        {
            public async Task<Result<GetBookForReadingResponse>> Handle(
                GetBookForReadingQuery request,
                CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);

                //if (user.Role.Equals("reader", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    var isSubActive = await subscriptionRepository.GetByReaderIdAsync(request.ReaderId, true);

                //    if (isSubActive is null)
                //        return Result<GetBookForReadingResponse>.Failure("You must subscribe to read book");
                //}

                if (book == null)
                {
                    return Result<GetBookForReadingResponse>
                        .Failure("Book not found.");
                }

                var progress = await readingProgressRepository
                    .GetAsync(request.ReaderId, request.BookId);

                    if (progress is null)
                    {
                        progress = new ReadingProgress
                        {
                            ReaderId = request.ReaderId,
                            BookId = request.BookId,
                            CurrentPage = 1,
                            CurrentChapter = "",
                            ProgressPercentage = 0,
                            LastReadDate = DateTime.UtcNow,
                            CreatedBy = request.ReaderId.ToString()
                        };

                        await readingProgressRepository.AddAsync(progress);
                    }

                var response = new GetBookForReadingResponse(
                    book.Id,
                    book.Title,
                    book.Subtitle,
                    book.Author,
                    book.BookCoverUrl,
                    book.BookFileUrl,
                    book.FileType,
                    book.AllowDownload,
                    book.AllowPrint,
                    book.AllowCopyPaste,
                    progress?.CurrentPage,
                    progress?.Cfi,
                    progress.ProgressPercentage
                );

                await unitOfWork.SaveAsync();
                return Result<GetBookForReadingResponse>
                    .Success(response, "Book retrieved successfully");
            }
        }
        public record GetBookForReadingResponse
(
    Guid Id,
    string Title,
    string? Subtitle,
    string Author,
    string BookCoverUrl,
    string BookFileUrl,
    string FileType,
    bool AllowDownload,
    bool AllowPrint,
    bool AllowCopyPaste,
    int? CurrentPage,
    string? Cfi,
    double ProgressPercentage
);
    }
}

using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddReadingProgress
    {
        public record AddReadingProgressCommand(
            Guid ReaderId, Guid BookId, double Percentage, string? CurrentLocation) : IRequest<Result<string>>;
        public class AddReadingProgressHandler(IReadingProgressRepository readingProgress) : IRequestHandler<AddReadingProgressCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddReadingProgressCommand request, CancellationToken cancellationToken)
            {
                var existingProgress = await readingProgress.GetAsync(request.ReaderId, request.BookId);

                if (existingProgress is null)
                {
                    await readingProgress.AddAsync(new ReadingProgress
                    {
                        ReaderId = request.ReaderId,
                        BookId = request.BookId,
                        ProgressPercentage = request.Percentage,
                        CurrentChapter = request.CurrentLocation
                    });
                }

                existingProgress!.ProgressPercentage = request.Percentage;
                existingProgress.CurrentChapter = request.CurrentLocation;
                existingProgress.LastReadDate = DateTime.UtcNow;

                return Result<string>.Success("Reading progress updated successfully.", "");
            }
        }
    }
}

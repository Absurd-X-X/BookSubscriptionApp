using Application.Common.Repositories;
using Application.Common.Dtos;
using MediatR;

namespace Application.Commands
{
    public class RemoveReadingProgress
    {
        public record RemoveReadingProgressCommand(
            Guid UserId,
            Guid ReadingProgressId
            ) : IRequest<Result<bool>>;

        public class RemoveReadingProgressHandler(
            IReadingProgressRepository readingProgressRepository
            ) : IRequestHandler<RemoveReadingProgressCommand, Result<bool>>
        {
            async Task<Result<bool>> IRequestHandler<RemoveReadingProgressCommand, Result<bool>>.
                Handle(RemoveReadingProgressCommand request, CancellationToken cancellationToken)
            {
                var userId = request.UserId;

                var progress = await readingProgressRepository.GetByIdAsync(request.ReadingProgressId);

                if (progress is null)
                {
                    return Result<bool>.Failure("Reading progress not found");
                }

                if (progress.ReaderId != userId)
                {
                    return Result<bool>.Failure("You do not have permission to remove this entry");
                }

                await readingProgressRepository.SoftDeleteAsync(request.ReadingProgressId);

                return Result<bool>.Success(true, "Removed");
            }
        }
    }
}
using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class RemoveFromReadingList
    {
        public record RemoveFromReadingListCommand(
            Guid ReaderId,
            Guid BookId
            ) : IRequest<Result<Unit>>;

        public class RemoveFromReadingListHandler(
            IReadingListRepository readingListRepository
            ) : IRequestHandler<RemoveFromReadingListCommand, Result<Unit>>
        {
            async Task<Result<Unit>> IRequestHandler<RemoveFromReadingListCommand, Result<Unit>>.
                Handle(RemoveFromReadingListCommand request, CancellationToken cancellationToken)
            {
                var existing = await readingListRepository.GetAsync(request.ReaderId, request.BookId);
                if (existing is null)
                    return Result<Unit>.Success(Unit.Value, "Not in reading list");

                await readingListRepository.RemoveAsync(existing);

                return Result<Unit>.Success(Unit.Value, "Removed from reading list");
            }
        }
    }
}
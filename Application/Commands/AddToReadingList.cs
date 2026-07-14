using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddToReadingList
    {
        public record AddToReadingListCommand(
            Guid ReaderId,
            Guid BookId
            ) : IRequest<Result<Unit>>;

        public class AddToReadingListHandler(
            IReadingListRepository readingListRepository
            ) : IRequestHandler<AddToReadingListCommand, Result<Unit>>
        {
            async Task<Result<Unit>> IRequestHandler<AddToReadingListCommand, Result<Unit>>.
                Handle(AddToReadingListCommand request, CancellationToken cancellationToken)
            {
                var existing = await readingListRepository.GetAsync(request.ReaderId, request.BookId);
                if (existing is not null)
                    return Result<Unit>.Success(Unit.Value, "Already in reading list");

                var item = new ReadingListItem
                {
                    Id = Guid.NewGuid(),
                    ReaderId = request.ReaderId,
                    BookId = request.BookId
                };

                await readingListRepository.AddAsync(item);

                return Result<Unit>.Success(Unit.Value, "Added to reading list");
            }
        }
    }
}
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.ReaderBooks.Commands.ToggleBookListMembership
{

    public sealed record ToggleBookListMembershipCommand(
        Guid ReaderId, Guid BookId, BookListType ListType) : IRequest<bool>;

    public sealed class ToggleBookListMembershipCommandHandler(
        IReadingListRepository readingListRepository,
        IFavoriteRepository favoriteRepository)
        : IRequestHandler<ToggleBookListMembershipCommand, bool>
    {
        public async Task<bool> Handle(ToggleBookListMembershipCommand request, CancellationToken ct)
        {
            if (request.ListType == BookListType.ReadingList)
            {
                var existing = await readingListRepository.GetAsync(request.ReaderId, request.BookId);
                if (existing is not null)
                {
                    await readingListRepository.RemoveAsync(existing);
                    return false;
                }

                await readingListRepository.AddAsync(new ReadingListItem
                {
                    Id = Guid.NewGuid(),
                    ReaderId = request.ReaderId,
                    BookId = request.BookId
                });
                return true;
            }

            // Favorite
            var existingFav = await favoriteRepository.GetAsync(request.ReaderId, request.BookId);
            if (existingFav is not null)
            {
                await favoriteRepository.RemoveAsync(existingFav);
                return false;
            }

            await favoriteRepository.AddAsync(new Favorite
            {
                Id = Guid.NewGuid(),
                ReaderId = request.ReaderId,
                BookId = request.BookId
            });
            return true;
        }
    }
}
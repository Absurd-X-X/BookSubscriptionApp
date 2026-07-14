using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class RemoveBookmark
    {
        public record RemoveBookmarkCommand(
            Guid ReaderId,
            Guid BookmarkId
            ) : IRequest<Result<Unit>>;

        public class RemoveBookmarkHandler(
            IBookmarkRepository bookmarkRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<RemoveBookmarkCommand, Result<Unit>>
        {
            async Task<Result<Unit>> IRequestHandler<RemoveBookmarkCommand, Result<Unit>>.
                Handle(RemoveBookmarkCommand request, CancellationToken cancellationToken)
            {
                var bookmark = await bookmarkRepository.GetByIdAsync(request.BookmarkId);

                if (bookmark is null || bookmark.ReaderId != request.ReaderId)
                    return Result<Unit>.Failure("Bookmark not found");

                await bookmarkRepository.RemoveAsync(bookmark);
                await unitOfWork.SaveAsync();

                return Result<Unit>.Success(Unit.Value, "Bookmark removed");
            }
        }
    }
}
using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class UpdateBookmarkNote
    {
        public record UpdateBookmarkNoteCommand(
            Guid BookmarkId,
            string Note
            ) : IRequest<Result<Unit>>;

        public class UpdateBookmarkNoteHandler(
            IBookmarkRepository bookmarkRepository
            ) : IRequestHandler<UpdateBookmarkNoteCommand, Result<Unit>>
        {
            async Task<Result<Unit>> IRequestHandler<UpdateBookmarkNoteCommand, Result<Unit>>.
                Handle(UpdateBookmarkNoteCommand request, CancellationToken cancellationToken)
            {
                var bookmark = await bookmarkRepository.GetByIdAsync(request.BookmarkId);
                if (bookmark is null)
                    return Result<Unit>.Failure("Bookmark not found");

                bookmark.Note = request.Note;

                return Result<Unit>.Success(Unit.Value, "Note updated");
            }
        }
    }
}
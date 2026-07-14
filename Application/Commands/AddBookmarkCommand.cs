using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddBookmark
    {
        public record AddBookmarkCommand(
            Guid ReaderId,
            Guid BookId,
            int PageNumber,
            string Quote,
            string Note
            ) : IRequest<Result<Guid>>;

        public class AddBookmarkHandler(
            IBookmarkRepository bookmarkRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<AddBookmarkCommand, Result<Guid>>
        {
            async Task<Result<Guid>> IRequestHandler<AddBookmarkCommand, Result<Guid>>.
                Handle(AddBookmarkCommand request, CancellationToken cancellationToken)
            {
                var bookmark = new Bookmark
                {
                    Id = Guid.NewGuid(),
                    ReaderId = request.ReaderId,
                    BookId = request.BookId,
                    PageNumber = request.PageNumber,
                    Quote = request.Quote,
                    Note = request.Note,
                    CreatedBy = request.ReaderId.ToString()
                };

                await bookmarkRepository.AddAsync(bookmark);
                await unitOfWork.SaveAsync();

                return Result<Guid>.Success(bookmark.Id, "Bookmark added");
            }
        }
    }
}
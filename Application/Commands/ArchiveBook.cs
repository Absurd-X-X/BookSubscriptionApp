using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class ArchiveBook
    {
        public record ArchiveBookCommand(Guid BookId) : IRequest<Result<string>>;

        public class ArchiveBookHandler(
            IBookRepository book,
            IUnitOfWork unitOfWork) : IRequestHandler<ArchiveBookCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ArchiveBookCommand request, CancellationToken cancellationToken)
            {
                var getBook = await book.GetByIdAsync(request.BookId);

                if (getBook is null)
                    return Result<string>.Failure("Book Not found");

                getBook.IsDeleted = true;
                await unitOfWork.SaveAsync();

                return Result<string>.Success("Successfully", "Archived!");
            }
        }
    }
}

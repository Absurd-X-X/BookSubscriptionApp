using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class UpdateBook
    {
        public record UpdateBookCommand(
            Guid BookId,
            string Title,
            string Author,
            int Pages,
            int PublicationYear,
            string Isbn
            ) : IRequest<Result<string>>;

        public class UpdateBookHandler(
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IUserRepository userRepository
            ) : IRequestHandler<UpdateBookCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);
                var userId = currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync(userId);

                if ( user == null ) 
                    return Result<string>.Failure("User not found");

                if (book == null)
                    return Result<string>.Failure("Book not found");

                if (Guid.Parse(book.CreatedBy) != user.Id)
                    return Result<string>.Failure("Unauthorized to do this");

                book.Title = request.Title;
                book.Author = request.Author;
                book.Pages = request.Pages;
                book.PublicationYear = request.PublicationYear;
                book.Isbn = request.Isbn;
                book.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}

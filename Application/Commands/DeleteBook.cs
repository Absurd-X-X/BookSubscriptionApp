using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class DeleteBook
    {
        public record DeleteBookCommand(Guid Id) : IRequest<Result<string>>;

        public class DeleteBookHandler(
            ICurrentUser _currentUser,
            IBookRepository _bookRepository,
            IUserRepository userRepository,
            IUnitOfWork _unitOfWork) : IRequestHandler<DeleteBookCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
            {
                var check = await _bookRepository.GetByIdAsync(request.Id);

                if (check == null)
                    return Result<string>.Failure("No book found");

                var getUser = _currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync(getUser);

                if (user == null)
                    return Result<string>.Failure("You've not logged in");

                if (check.CreatedBy != user.Email)
                    return Result<string>.Failure("You don't have permission to delete this book");
                
                check.IsDeleted = true;
                check.DateModified = DateTime.UtcNow;

                await _unitOfWork.SaveAsync();

                return Result<string>.Success("Book", "Deleted");
            }
        }
    }
}

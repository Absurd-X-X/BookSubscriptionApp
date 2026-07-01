using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class UpdateLibraryDetails
    {
        public record UpdateLibraryComand(
            Guid UserId,
            string Name,
            string Email,
            string PhoneNumber,
            string Username
            ) : IRequest<Result<string>>;

        public class UpdateLibraryHandler(
            ILibraryRepository libraryRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository
            ) : IRequestHandler<UpdateLibraryComand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateLibraryComand request, CancellationToken cancellationToken)
            {

                var user = await userRepository.GetAsync(request.UserId); 

                if (user == null)
                    return Result<string>.Failure("User not found");

                var library = await libraryRepository.GetLibraryAsync(user.Email);

                if (library == null)
                    return Result<string>.Failure("Library not found");

                if (library.CreatedBy != user.Email)
                    return Result<string>.Failure("Unauthorized");
                user.UserName = request.Username; 
                user.Email = request.Email;
                library.Name = request.Name;
                library.Email = request.Email;
                library.PhoneNumber = request.PhoneNumber;
                library.CreatedBy = request.Email;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}

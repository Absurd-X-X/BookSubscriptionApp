using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class UpdateReaderDetails
    {
        public record UpdateReaderCommand(
            Guid UserId,
            string Name,
            string Email
            ) : IRequest<Result<string>>;

        public class UpdateReaderHandler(
            IReaderRepository readerRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<UpdateReaderCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateReaderCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null)
                    return Result<string>.Failure("Unauthorized");

                var reader = await readerRepository.GetByEmailAsync(user.Email);

                if (reader is null)
                    return Result<string>.Failure("Reader not found");

                if (reader.CreatedBy != user.Email)
                    return Result<string>.Failure("Unauthorized");

                reader.Name = request.Name;
                reader.Email = request.Email;
                reader.CreatedBy = request.Email;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}

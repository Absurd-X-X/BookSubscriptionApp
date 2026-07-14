using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class ChangePassword
    {
        public record ChangePasswordCommand(Guid UserId, string InitialPassword, string NewPassword) : IRequest<Result<string>>;
        public class ChangePasswordHandler(IPasswordHasher<string> passwordHasher,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository) : IRequestHandler<ChangePasswordCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user == null)
                {
                    return Result<string>.Failure("User Not Found");
                }

                var verify = passwordHasher.VerifyHashedPassword(user.Email, user.HashPassword, request.InitialPassword);

                if (verify == PasswordVerificationResult.Failed)
                    return Result<string>.Failure("Incorrect Password");

                var hash = passwordHasher.HashPassword(user.Email, request.NewPassword);
                user.HashPassword = hash;


                await unitOfWork.SaveAsync();
                return Result<string>.Success("Password changed successfully", "Changed!");
            }
        }
    }
}

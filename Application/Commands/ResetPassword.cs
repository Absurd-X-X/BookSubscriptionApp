using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class ResetPassword
    {
        public record ResetPasswordCommand(
            string Email,
            string Token,
            string NewPassword,
            string ConfirmPassword)
            : IRequest<Result<string>>;

        

        public class ResetPasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWork unitOfWork,
            IAuditLogRepository auditLogRepository)
            : IRequestHandler<ResetPasswordCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                ResetPasswordCommand request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user is null)
                    return Result<string>.Failure("Invalid request");

                if (user.VerificationToken != request.Token)
                    return Result<string>.Failure("Invalid reset code");

                if (user.VerificationTokenExpiry < DateTime.UtcNow)
                    return Result<string>.Failure(
                        "Reset code has expired. Please request a new one");

                user.HashPassword = passwordHasher.HashPassword(user, request.NewPassword);
                user.VerificationToken = null;
                user.VerificationTokenExpiry = null;
                user.DateModified = DateTime.UtcNow;


                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Password re-set",
                    Icon = "🔑",
                    Description = $"User Added: {user.Library?.Name ?? user.Reader?.Name}({user.UserName})",
                    IpAddress = "",
                    UserRole = user.Role
                });

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Password reset successfully! You can now login.", "reset");
            }
        }
    }
}
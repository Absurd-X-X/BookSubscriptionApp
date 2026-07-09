using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class VerifyEmail
    {
        public record VerifyEmailCommand(
            string Email,
            string Token)
            : IRequest<Result<string>>;

        

        public class VerifyEmailHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IAuditLogRepository auditLogRepository)
            : IRequestHandler<VerifyEmailCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                VerifyEmailCommand request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user is null)
                    return Result<string>.Failure("User not found");

                if (user.IsVerified)
                    return Result<string>.Failure("Email already verified");

                if (user.VerificationToken != request.Token)
                    return Result<string>.Failure("Invalid verification code");

                if (user.VerificationTokenExpiry < DateTime.UtcNow)
                    return Result<string>.Failure(
                        "Verification code has expired. Please request a new one");

                user.IsVerified = true;
                user.VerificationToken = null;
                user.VerificationTokenExpiry = null;
                user.DateModified = DateTime.UtcNow;


                var emailResult = await emailService.SendEmailAsync(
                    user.Email,
                    "Welcome to BookSubscriptionApp 🎉",
                    EmailTemplates.WelcomeEmail(user.UserName ?? "Customer"));

                if (!emailResult.Success)
                    return Result<string>.Failure("Failed to send welcome email due to network error");

                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Verify Email",
                    Icon = "🚦",
                    Description = $"Verify Email : {user.Library?.Name ?? user.Reader?.Name}({user.UserName})",
                    IpAddress = "",
                    UserRole = user.Role,
                    ResourceType = ResourceType.System,
                    ResourceId = user.Id,
                });

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Email verified successfully! You can now login.", "verified");
            }
        }
    }
}
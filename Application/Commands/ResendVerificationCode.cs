using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class ResendVerification
    {
        public record ResendVerificationCommand(
            string Email)
            : IRequest<Result<string>>;

        public class ResendVerificationHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<ResendVerificationCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                ResendVerificationCommand request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user is null)
                    return Result<string>.Failure("User not found");

                if (user.IsVerified)
                    return Result<string>.Failure("Email already verified");

                var token = new Random().Next(1000, 9999).ToString();

                user.VerificationToken = token;
                user.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5);
                user.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();

                var emailResult = await emailService.SendEmailAsync(
                    user.Email,
                    "New Verification Code",
                    EmailTemplates.VerificationEmail(
                        user.UserName ?? "Customer", token));

                if (!emailResult.Success)
                    return Result<string>.Failure("Failed to send verification email due to network error");


                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Resend",
                    Description = $"Verification code resent to {user.Email}",
                    Icon = "🔄",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = null
                };

                await auditLogRepository.AddAsync(audit);

                return Result<string>.Success(
                    "New verification code sent to your email", "sent");
            }
        }
    }
}
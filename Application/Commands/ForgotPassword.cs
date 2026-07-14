using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class ForgotPassword
    {
        public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;

        public class ForgotPasswordHAndler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailServices,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<ForgotPasswordCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);

                if (user is null)
                    return Result<string>.Failure("The inserted email is incorrect");

                var token = new Random().Next(1, 9999).ToString();

                user.VerificationToken = token;
                user.DateModified = DateTime.UtcNow;
                user.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5);



                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Reset Password",
                    Description = $"Notification was archived successfully",
                    Icon = "🔄",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = null,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();

                await emailServices.SendEmailAsync(
                    user.Email, 
                    "Verify Your Email",
                    EmailTemplates.ForgotPasswordEmail(user.UserName, token));

                return Result<string>.Success("You will shortly recieve a reset code if this email exists", "sent");

            }
        }
    }
}

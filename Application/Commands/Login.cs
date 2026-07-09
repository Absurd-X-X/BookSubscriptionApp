using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands
{
    public class Login
    {
        public record LoginCommand(
            string Email,
            string Password,
            bool RememberMe
            ) : IRequest<Result<LoginResponse>>;

        public class LoginHandler(
            IUserRepository userRepository,
            IPasswordHasher<string> passwordHasher,
            IReaderRepository readerRepository,
            ILibraryRepository libraryRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<LoginCommand, Result<LoginResponse>>
        {
            public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Email);
                if (user is null)
                    return Result<LoginResponse>.Failure("Invalid credentials");

                if (!user.IsVerified)
                    return Result<LoginResponse>.Failure("You must be verified before logging in.");

                var passwordCheck = passwordHasher.VerifyHashedPassword(user.Email, user.HashPassword, request.Password);
                if (passwordCheck == PasswordVerificationResult.Failed)
                    return Result<LoginResponse>.Failure("Invalid credentials");

                string customerId = user.Id.ToString();

                if (user.Role != "admin")
                {
                    var reader = await readerRepository.GetByEmailAsync(user.Email);
                    if (reader is not null)
                    {
                        customerId = reader.Id.ToString();
                    }
                    else
                    {
                        var library = await libraryRepository.GetLibraryAsync(user.Email);
                        if (library is null)
                        {
                            return Result<LoginResponse>.Failure("Associated profile record not found.");
                        }
                        customerId = library.Id.ToString();
                    }
                }

                var response = new LoginResponse
                (
                    UserId: user.Id,
                    CustomerId: customerId,
                    ProfileUrl: user.ImageUrl ?? "save later",
                    FullName: user.UserName ?? "User",
                    Email: user.Email,
                    Role: user.Role
                );

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Logged In",
                    Icon = "🔑",
                    Description = $"User Added: {user.Library?.Name ?? user.Reader?.Name}({user.UserName})",
                    IpAddress = "",
                    UserRole = user.Role,
                    ResourceType = ResourceType.System,
                    ResourceId = null,
                });

                await unitOfWork.SaveAsync();

                return Result<LoginResponse>.Success(response, "Successfully logged in");
            }
        }

        public record LoginResponse(
            Guid UserId,
            string CustomerId,
            string? ProfileUrl,
            string FullName,
            string Email,
            string Role);
    }
}

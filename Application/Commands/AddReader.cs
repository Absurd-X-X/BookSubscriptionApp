using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Command
{
    public class AddReader
    {
        public record AddReaderCommand(
            string Name,
            string Email,
            string Password,
            string UserName
            ) : IRequest<Result<AddReaderResponse>>;

        public class AddReaderHAndler(
            IUserRepository userRepository,
            IReaderRepository readerRepository,
            IUnitOfWork unitOfWork,
            IWalletRepository walletRepository,
            IEmailService emailServices,
            IPasswordHasher<string> passwordHasher,
            IAuditLogRepository auditLogRepository) : IRequestHandler<AddReaderCommand, Result<AddReaderResponse>>
        {
            public async Task<Result<AddReaderResponse>> Handle(AddReaderCommand request, CancellationToken cancellationToken)
            {
                var check = await userRepository.IsExistAsync(request.Email);

                if (check)
                    return Result<AddReaderResponse>.Failure("Already exists");

                var token = new Random().Next(1, 9999).ToString();

                var user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    Role = "reader",
                    HashPassword = passwordHasher.HashPassword(request.Email, request.Password),
                    IsVerified = false,
                    VerificationTokenExpiry = DateTime.UtcNow.AddDays(5),
                    VerificationToken = token,
                    CreatedBy = request.Email
                };

                await userRepository.AddAsync(user);

                Reader reader = new Reader
                {
                    Name = request.Name,
                    Email = request.Email,
                    CreatedBy = user.Id.ToString(),
                    UserId = user.Id
                };
                await readerRepository.AddAsync(reader);


                await walletRepository.AddAsync(new Wallet
                {
                    UserId = user.Id,
                    Balance = 0,
                    CreatedBy = user.Id.ToString(),
                    DateCreated = DateTime.UtcNow
                });


                var emailResult = await emailServices.SendEmailAsync(
                    user.Email, 
                    "Verification Token", 
                    EmailTemplates.VerificationEmail(request.Name, user.VerificationToken));

                if (!emailResult.Success)
                    return Result<AddReaderResponse>.Failure("Failed to send verification email due to network error");

                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Added New User",
                    Icon = "👤",
                    Description = $"User Added: {request.Name}({request.UserName})",
                    IpAddress = "",
                    UserRole = user.Role,
                    ResourceType = ResourceType.Reader,
                    ResourceId = reader.Id
                });

                await unitOfWork.SaveAsync();

                return Result<AddReaderResponse>.Success(new AddReaderResponse(user.Id, user.Email), 
                    "Registration successful! Please check your email for verification code.");
            }
        }
    }

    public record AddReaderResponse(Guid Id, string Email);
}

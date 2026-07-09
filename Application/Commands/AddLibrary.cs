using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Command
{
    public class AddLibrary
    {
        public record AddLibraryCommand(
            string Name,
            string Email,
            string PhoneNumber,
            string UserName,
            string Password) : IRequest<Result<AddLibraryResponse>>;

        public class AddLibraryHandler(
            ILibraryRepository libraryRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IEmailService emailServices,
            IPasswordHasher<User> passwordHasher,
            IAuditLogRepository auditLogRepository) : 
            IRequestHandler<AddLibraryCommand, Result<AddLibraryResponse>>
        {
            public async Task<Result<AddLibraryResponse>> Handle(AddLibraryCommand request, CancellationToken cancellationToken)
            {
                var check = await userRepository.IsExistAsync(request.Email);

                if (check)
                    return Result<AddLibraryResponse>.Failure("User already exists");

                var token = new Random().Next(1000, 9999).ToString();

                var user = new User
                {
                    Email = request.Email,
                    IsVerified = false,
                    VerificationToken = token,
                    VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5),
                    UserName = request.UserName,
                    CreatedBy = request.UserName,
                    Role = "library"
                };

                user.HashPassword = passwordHasher.HashPassword(user, request.Password);

                await userRepository.AddAsync(user);

                string cleanRef = request.Name.Replace(" ", "");
                string shortCode = cleanRef.Length >= 3 ? cleanRef[..3] : "LIB";

                Library library = new Library
                {
                    Name = request.Name,
                    Email = request.Email,
                    UserId = user.Id,
                    PhoneNumber = request.PhoneNumber,
                    RefNumber = $"Absourd{shortCode.ToUpper()}",
                    CreatedBy = user.Id.ToString()
                };

                await libraryRepository.AddAsync(library);

                await walletRepository.AddAsync(new Wallet
                {
                    Balance = 0,
                    CreatedBy = user.Id.ToString(),
                    UserId = user.Id
                });


                var emailResult = await emailServices.SendEmailAsync(
                    user.Email,
                    "Verify your email",
                    EmailTemplates.VerificationEmail(request.Name, token));

                if (!emailResult.Success)
                    return Result<AddLibraryResponse>.Failure("Failed to send verification email due to network error");

                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Added New User",
                    Icon = "👤",
                    Description = $"User Added: {request.Name}({request.UserName})",
                    IpAddress = "",
                    UserRole = user.Role,
                    ResourceType = ResourceType.System,
                    ResourceId = library.Id
                });

                await unitOfWork.SaveAsync();

                return Result<AddLibraryResponse>.Success(new AddLibraryResponse(user.Id, user.Email), "Successfully Added");
            }
        }
    }
    public record AddLibraryResponse(Guid Id, string Email);
}

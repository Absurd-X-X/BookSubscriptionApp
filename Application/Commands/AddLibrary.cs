using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static Application.Commands.StartConversation.StartConversationHandler;

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
            IConversationRepository conversationRepository,
            IAuditLogRepository auditLogRepository) : 
            IRequestHandler<AddLibraryCommand, Result<AddLibraryResponse>>
        {
            public async Task<Result<AddLibraryResponse>> Handle(AddLibraryCommand request, CancellationToken cancellationToken)
            {
                var check = await userRepository.IsExistAsync(request.Email);

                if (check)
                    return Result<AddLibraryResponse>.Failure("User already exists");

                var admin = await userRepository.GetAsync("admin@gmail.com");

                if (admin == null)
                    return Result<AddLibraryResponse>.Failure("You can't add this library");

                var token = new Random().Next(1000, 9999).ToString();

                var user = new User
                {
                    Email = request.Email,
                    IsVerified = false,
                    VerificationToken = token,
                    VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5),
                    UserName = request.UserName,
                    CreatedBy = admin.Id.ToString(),
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


                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Added New User",
                    Icon = "👤",
                    Description = $"Super Admin created library '{request.Name}' ({request.UserName})",
                    IpAddress = "",
                    UserRole = user.Role,
                    ResourceType = ResourceType.System,
                    ResourceId = library.Id
                });

                if (admin is null)
                    return Result<AddLibraryResponse>.Failure("Something weny wrong");

                var getGroupConversation = await conversationRepository.GetGroupAsync(admin.Id);

                if (getGroupConversation != null)
                {
                    getGroupConversation.UserConversations.Add(
                        new UserConversation
                        {
                            UserId = user.Id
                        });
                }


                var checkConversation = await conversationRepository.GetPrivateConversationAsync(user.Id, admin.Id);

                if (checkConversation is null)
                {
                    var conversation = new Conversation
                    {
                        Title = request.UserName ?? request.Name,
                        CreatedBy = admin.Id.ToString(),
                        LastMessageAt = null,
                        UserConversations = new List<UserConversation>
                     {
                         new UserConversation
                         {
                             UserId = admin.Id,
                         },

                         new UserConversation
                         {
                             UserId = user.Id,
                         }
                     },
                    };


                    await conversationRepository.AddAsync(conversation);
                }


                await unitOfWork.SaveAsync();


                var emailResult = await emailServices.SendEmailAsync(
                    user.Email,
                    "Verify your email",
                    EmailTemplates.VerificationEmail(request.Name, token));

                if (!emailResult.Success)
                    return Result<AddLibraryResponse>.Failure("Failed to send verification email due to network error");

                return Result<AddLibraryResponse>.Success(new AddLibraryResponse(user.Id, user.Email), "Successfully Added");
            }
        }
    }
    public record AddLibraryResponse(Guid Id, string Email);
}

using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class UpdateLibraryDetails
    {
        public record UpdateLibraryComand(
            Guid UserId,
            string Name,
            string Email,
            string PhoneNumber,
            string Username
            ) : IRequest<Result<string>>;

        public class UpdateLibraryHandler(
            ILibraryRepository libraryRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<UpdateLibraryComand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateLibraryComand request, CancellationToken cancellationToken)
            {

                var library = await libraryRepository.GetAsync(request.UserId);

                var admin = await userRepository.GetAsync("admin@gmail.com");

                if (admin == null)
                    return Result<string>.Failure("User not found");

                if (library == null)
                    return Result<string>.Failure("Library not found");

                if (library.CreatedBy != admin.Id.ToString())
                    return Result<string>.Failure("Unauthorized");

                library.User.UserName = request.Username; 
                library.User.Email = request.Email; 
                library.Name = request.Name;
                library.Email = request.Email;
                library.PhoneNumber = request.PhoneNumber;
                library.CreatedBy = request.Email;

                var user = await userRepository.GetAsync(request.Email);
                if (user == null)
                    return Result<string>.Failure("User not found");

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Update",
                    Description = $"library Details updated successfully",
                    Icon = "🏷️",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = library.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}

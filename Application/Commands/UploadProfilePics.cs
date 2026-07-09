using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class UploadProfilePIcs
    {
        public record UploadProfilePicsCommand(Guid UserId, Stream ProfilePic, string FileName) : IRequest<Result<string>>;
        public class UploadProfilePicsHandler(IUnitOfWork _unitOfWork, IUserRepository userRepository,
            IAuditLogRepository auditLogRepository, IHttpContextAccessor httpContextAccessor) : IRequestHandler<UploadProfilePicsCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UploadProfilePicsCommand request, CancellationToken cToken)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profilepics");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var ext = Path.GetExtension(request.FileName);

                var unique = $"{Guid.NewGuid()}_{request.UserId}{ext}";
                var combine = Path.Combine(folder, unique);

                using (var fileStream = new FileStream(combine, FileMode.Create))
                {
                    await request.ProfilePic.CopyToAsync(fileStream);
                }
                var user = await userRepository.GetAsync(request.UserId);
                if (user == null)
                {
                    return Result<string>.Failure("User not found");
                }

                user.ImageUrl = $"/uploads/profilepics/{unique}";



                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Create",
                    Description = $"Notification was archived successfully",
                    Icon = "🔔",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = null,
                };

                await auditLogRepository.AddAsync(audit);

                await _unitOfWork.SaveAsync();

                return Result<string>.Success(combine, "Done");
            }
        }
    }
}

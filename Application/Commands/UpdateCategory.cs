using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class UpdateCategory
    {
        public record UpdateCategoryCommand(
            Guid CategoryId,
            Guid UserId,
            string Name,
            string Description
            ) : IRequest<Result<string>>;

        public class UpdateCategoryHandler(
            ICategoryRepository categoryRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<UpdateCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.CategoryId);

                var user = await userRepository.GetAsync(request.UserId);

                if (user == null) 
                    return Result<string>.Failure("User not found");

                if (category == null)
                    return Result<string>.Failure("Not found");

                category.Name = request.Name;
                category.Description = request.Description;
                category.DateModified = DateTime.UtcNow;


                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Update",
                    Description = $"Category was updated successfully",
                    Icon = "🏷️",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = category.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");

            }
        }
    }
}

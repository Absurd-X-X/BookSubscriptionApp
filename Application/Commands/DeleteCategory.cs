using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Application.Commands
{
    public class DeleteCategory
    {
        public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<Result<string>>;

        public class DeleteCategoryHandler(
            ICategoryRepository categoryRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.Id);

                if (user is null)
                    return Result<string>.Failure("User Not found");

                var category = await categoryRepository.GetCategoryAsync(request.Id);

                if (category == null)
                    return Result<string>.Failure("Category not found");
                    
                category.IsDeleted = true;
                category.DateModified = DateTime.UtcNow;

                string? ipAddress = httpContextAccessor
              .HttpContext?
              .Connection
              .RemoteIpAddress?
              .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Delete",
                    Description = $"{category.Name} was deleted successfully",
                    Icon = "❌",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = category.Id
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Category", "Deleted");
            }
        }
    }
}

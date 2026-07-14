using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class AddCategory
    {
        public record AddCategoryCommand(
            string Name,
            string Description) : IRequest<Result<string>>;

        public class AddCategoryHandler(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            ICurrentUser currentUser) : 
            IRequestHandler<AddCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
            {
                var check = await categoryRepository.IsExistAsync(request.Name);

                if (check)
                    return Result<string>.Failure("Already Exists");

                var getUser = currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync(getUser);

                if (user == null)
                    return Result<string>.Failure("User not found");

                var category = new Category
                {
                     Name = request.Name,
                     Description = request.Description,
                     CreatedBy = user.Email
                };

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Create",
                    Description = $"{category.Name} was added successfully",
                    Icon = "📖",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    ResourceType = ResourceType.System,
                    ResourceId = category.Id,
                    UserId = user.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await categoryRepository.AddAsync(category);

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Category", "Added");
            }
        }
    }
}

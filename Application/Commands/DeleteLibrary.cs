using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class DeleteLibrary
    {
        public record DeleteLibraryCommand(Guid LibraryId) : IRequest<Result<string>>;

        public class DeleteLibraryHandler(
            ILibraryRepository libraryRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<DeleteLibraryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteLibraryCommand request, CancellationToken cancellationToken)
            {
                var library = await libraryRepository.GetAsync(request.LibraryId);

                if (library is null)
                    return Result<string>.Failure("Library not found");

                library.IsDeleted = true;
                library.DateModified = DateTime.UtcNow;

               string? ipAddress = httpContextAccessor
              .HttpContext?
              .Connection
              .RemoteIpAddress?
              .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Delete",
                    Description = $"{library.Name}'s sub was renewed successfully",
                    Icon = "❌",
                    IpAddress = ipAddress!,
                    UserRole = library.User.Role,
                    UserId = library.UserId,
                    ResourceType = ResourceType.System,
                    ResourceId = library.Id
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Deleted", "Successfully");
            }
        }
    }
}

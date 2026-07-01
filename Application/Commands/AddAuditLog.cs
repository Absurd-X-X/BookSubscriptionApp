using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddAuditLog
    {
        public record AddAuditLogCommand(
            Guid Id, string UserRole, string ActionType, string Description, string IpAddress) : IRequest<Result<string>>;

        public class AddAuditLogHandler(IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<AddAuditLogCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddAuditLogCommand request, CancellationToken cancellationToken)
            {
                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = request.Id,
                    ActionType = request.ActionType,
                    UserRole = request.UserRole,
                    Description = request.Description,
                    IpAddress = request.IpAddress
                });

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Added", "Successfully");
            }
        }
    }
}

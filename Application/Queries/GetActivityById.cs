using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetActivityById
    {
        public record GetActivityByIdQuery(Guid Id) : IRequest<Result<GetActivityByIdResponse>>;

        public class GetActivityByIdHandler(IAuditLogRepository auditLog) : IRequestHandler<GetActivityByIdQuery, Result<GetActivityByIdResponse>>
        {
            public async Task<Result<GetActivityByIdResponse>> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
            {
                var activity = await auditLog.GetByIdAsync(request.Id);
                if (activity == null)
                {
                    return Result<GetActivityByIdResponse>.Failure("No log found!");
                }
                var response = new GetActivityByIdResponse(
                    activity.Id,
                    activity.Icon,
                    activity.UserRole,
                    activity.ActionType,
                    activity.Description,
                    activity.Timestamp,
                    activity.User.UserName,
                    activity.User.Role,
                    activity.User.ImageUrl ?? "none",
                    activity.User.DateCreated,
                    activity.User.Email,
                    activity.IpAddress
                );
                return Result<GetActivityByIdResponse>.Success(response, "Retrieved");
            }
        }   
        public record GetActivityByIdResponse(
            Guid Id,
            string Icon,
            string UserRole,
            string ActionType,
            string Description,
            DateTime Timestamp,
            string UserName,
            string Role,
            string ImageUrl,
            DateTime DateJoined,
            string Email,
            string IpAddress);
    }
}

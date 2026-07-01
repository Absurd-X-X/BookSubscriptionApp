using Application.Common.Dtos;
using Application.Common.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetNotificationById
    {
        public record GetNotificationByIdQuery(Guid Id) : IRequest<Result<GetNotificationByIdResponse>>;

        public class GetNotificationByIdHandler(INotificationRepository notificationRepository) : 
            IRequestHandler<GetNotificationByIdQuery, Result<GetNotificationByIdResponse>>
        {
            public async Task<Result<GetNotificationByIdResponse>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
            {
                var notification = await notificationRepository.GetById(request.Id);

                if (notification is null)
                    return Result<GetNotificationByIdResponse>.Failure("Not found");

                return Result<GetNotificationByIdResponse>.Success(notification.Adapt<GetNotificationByIdResponse>(), "Retrieved");
            }
        }

        public record GetNotificationByIdResponse(
            Guid Id, string Title, string Message,
            string Type, bool IsRead,
            string Ref, DateTime DateCreated);
    }
}

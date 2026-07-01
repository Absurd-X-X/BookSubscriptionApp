using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using MySqlX.XDevAPI.Common;

namespace Application.Queries
{
    public class GetSubscriptionById
    {
        public record GetSubscriptionByIdQuery(Guid Id) : IRequest<Result<GetSubscriptionByIdResponse>>;

        public class GetSubscriptionByIdHandler(
            ISubscriptionRepository subscriptionRepository
            ) : IRequestHandler<GetSubscriptionByIdQuery, Result<GetSubscriptionByIdResponse>>
        {
            public async Task<Result<GetSubscriptionByIdResponse>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
            {
                var subscription = await subscriptionRepository.GetAsync(request.Id);

                if (subscription is null)
                {
                    return Result<GetSubscriptionByIdResponse>.Failure("Subscription not found");
                }

                var subscriptionData = new GetSubscriptionByIdResponse(
                    subscription.Id,
                    subscription.AutoRenewal,
                    subscription.Reader.Id,
                    subscription.Reader.Name,
                    subscription.SubscriptionTypeId,
                    subscription.Types.Cost,
                    subscription.Types.SubscriptionDate,
                    subscription.Types.ExpiryDate,
                    subscription.IsActive
                    );
                return Result<GetSubscriptionByIdResponse>.Success(subscriptionData, "Retrieved");
            }
        }

        public record GetSubscriptionByIdResponse(
            Guid Id, 
            bool AutoRenew, 
            Guid ReaderId, 
            string ReaderName, 
            Guid SubscriptionTypeId, 
            decimal Cost, 
            DateTime SubcriptionDate, 
            DateTime ExpiryDate, 
            bool IsActive);
        }
    }


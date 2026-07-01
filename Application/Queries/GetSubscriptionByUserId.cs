using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using MySqlX.XDevAPI.Common;

namespace Application.Queries
{
    public class GetSubscriptionByUserId
    {
        public record GetSubscriptionByUserIdQuery(Guid UserId) : IRequest<Result<GetSubscriptionByUserIdResponse>>;

        public class GetSubscriptionByIdHandler(
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository,
            IReaderRepository readerRepository
            ) : IRequestHandler<GetSubscriptionByUserIdQuery, Result<GetSubscriptionByUserIdResponse>>
        {
            public async Task<Result<GetSubscriptionByUserIdResponse>> Handle(GetSubscriptionByUserIdQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null)
                    return Result<GetSubscriptionByUserIdResponse>.Failure("User not found");

                var reader = await readerRepository.GetByEmailAsync(user.Email);

                if (reader is null)
                    return Result<GetSubscriptionByUserIdResponse>.Failure("Reader not found");

                var subscription = await subscriptionRepository.GetByReaderIdAsync(reader.Id, true);

                if (subscription is null)
                {
                    return Result<GetSubscriptionByUserIdResponse>.Failure("Subscription not found");
                }

                var subscriptionData = new GetSubscriptionByUserIdResponse(
                    subscription.Id,
                    subscription.AutoRenewal,
                    subscription.Reader.Id,
                    subscription.Reader.Name,
                    subscription.SubscriptionTypeId,
                    subscription.Types.Cost,
                    subscription.Types.SubscriptionDate,
                    subscription.Types.ExpiryDate,
                    subscription.Types.TypeName,
                    subscription.Reader.Email,
                    subscription.Reader.DateCreated,
                    subscription.IsActive
                    );
                return Result<GetSubscriptionByUserIdResponse>.Success(subscriptionData, "Retrieved");
            }
        }

        public record GetSubscriptionByUserIdResponse(
            Guid Id, 
            bool AutoRenew, 
            Guid ReaderId, 
            string ReaderName, 
            Guid SubscriptionTypeId, 
            decimal Cost, 
            DateTime SubcriptionDate, 
            DateTime ExpiryDate, 
            string CurrentPlan,
            string ReaderEmail,
            DateTime ReaderAddedDate,
            bool IsActive);
        }
    }


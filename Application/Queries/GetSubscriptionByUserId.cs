using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetSubscriptionByUserId
    {
        public record GetSubscriptionByUserIdQuery(Guid UserId) : IRequest<Result<GetSubscriptionByUserIdResponse>>;

        public class GetSubscriptionByIdHandler(
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository,
            ISubscriptionTypeRepository subscriptionTypeRepository,
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

                // Fetch available plans regardless of whether the reader has an active subscription —
                // they need this list to pick a plan when they have none.
                var types = await subscriptionTypeRepository.GetAllAsync();

                var subscription = await subscriptionRepository.GetByReaderIdAsync(reader.Id, true);

                var userSubs = await subscriptionRepository.GetByReaderIdAsync(reader.Id);

                if (subscription is null)
                {
                    var noSubData = new GetSubscriptionByUserIdResponse(
                        Guid.Empty,
                        false,
                        reader.Id,
                        reader.Name,
                        Guid.Empty,
                        0m,
                        default,
                        [..types],
                        [..userSubs],
                        default,
                        string.Empty,
                        reader.Email,
                        reader.DateCreated,
                        false
                        );

                    return Result<GetSubscriptionByUserIdResponse>.Success(noSubData, "No active subscription");
                }

                var subscriptionData = new GetSubscriptionByUserIdResponse(
                    subscription.Id,
                    subscription.AutoRenewal,
                    subscription.Reader.Id,
                    subscription.Reader.Name,
                    subscription.SubscriptionTypeId,
                    subscription.Types.Cost,
                    subscription.Types.SubscriptionDate,
                    [..types],
                    [..userSubs], // Subscription history — still pending
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
            List<SubscriptionType>? AvailablePlans,
            List<Subscription>? Subscription,
            DateTime ExpiryDate,
            string CurrentPlan,
            string ReaderEmail,
            DateTime ReaderAddedDate,
            bool IsActive);
    }
}
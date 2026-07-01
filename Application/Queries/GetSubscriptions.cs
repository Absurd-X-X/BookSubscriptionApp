using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetSubscriptions
    {
        public record GetSubscriptionsQuery() : IRequest<Result<PagenatedList<GetSubscriptionsResponse>>>;

        public class GetSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) : IRequestHandler<GetSubscriptionsQuery, Result<PagenatedList<GetSubscriptionsResponse>>>
        {
            public async Task<Result<PagenatedList<GetSubscriptionsResponse>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
            {
                var subscriptions = await subscriptionRepository.GetSubscriptionsAsync(true, new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                });


                var response = subscriptions.Items.Select(x => new GetSubscriptionsResponse(
                    x.Id,
                    x.Types.Cycle switch
                    {
                        BillingCycle.Monthly => "Monthly",
                        BillingCycle.Quaterly => "Quarterly",
                        BillingCycle.SemiAnnually => "Semi Annually (Half a year)",
                        BillingCycle.Yearly => "Yearly",

                        _ => throw new ArgumentOutOfRangeException(nameof(x.Types.Cycle), $"Unknown billing cycle: {x.Types.Cycle} detected!!!")
                    },
                x.AutoRenewal,
                    x.ReaderId,
                    x.Reader.Name,
                    x.Reader.Email,
                    x.SubscriptionTypeId,
                    x.Types.Cost,
                    x.Types.SubscriptionDate,
                    x.Types.ExpiryDate,
                    x.IsActive)).ToList();

                return Result<PagenatedList<GetSubscriptionsResponse>>.Success(new PagenatedList<GetSubscriptionsResponse>
                {
                    Items = response,
                    TotalCount = subscriptions.TotalCount,
                    Page = subscriptions.Page,
                    PageSize = subscriptions.PageSize
                }, "Retrieved");
            }
        }

        public record GetSubscriptionsResponse(
            Guid Id,
            string BillingCycle,
            bool AutoRenew,
            Guid ReaderId,
            string ReaderName,
            string ReaderEmail,
            Guid SubscriptionTypeId,
            decimal Cost,
            DateTime SubcriptionDate,
            DateTime ExpiryDate,
            bool IsActive);
    }
}

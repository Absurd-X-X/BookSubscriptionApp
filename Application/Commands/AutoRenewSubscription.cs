using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class AutoRenewSubscription
    {
        public record AutoRenewCommand(bool AutoRenew, Guid Id) : IRequest<Result<string>>;

        public class AutoRenewHandler(IUnitOfWork unitOfWork,
            ISubscriptionRepository subscriptionRepository) : IRequestHandler<AutoRenewCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AutoRenewCommand request, CancellationToken cancellationToken)
            {
                var sub = await subscriptionRepository.GetAsync(request.Id);

                if (sub == null)
                {
                    return Result<string>.Failure("Not found brosky");
                }

                sub.AutoRenewal = true;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Retrieved", "Brosky");
            }
        }
    }
}

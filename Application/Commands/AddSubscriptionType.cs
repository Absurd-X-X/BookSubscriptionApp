using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class AddSubscriptionType
    {
        public record AddSubscriptionTypeCommand(Guid UserId, string TypeName, BillingCycle Cycle, decimal Cost) : IRequest<Result<string>>;

        public class AddSubscriptionTypeHandler(
            ISubscriptionTypeRepository subscriptionTypeRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<AddSubscriptionTypeCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddSubscriptionTypeCommand request, CancellationToken cancellationToken)
            {
                var type = await subscriptionTypeRepository.IsExistAsync(request.TypeName, request.Cycle);

                if (type is not null)
                    return Result<string>.Failure("Type already exists you can try updating to modify it");


                await subscriptionTypeRepository.AddAsync(new SubscriptionType
                {
                    TypeName = request.TypeName,
                    Cost = request.Cost,
                    CreatedBy = request.UserId.ToString(),
                    Cycle = request.Cycle
                });

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Subscription Type", "Created Successfully");
            }
        }
    }
}

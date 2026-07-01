using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetReaderById
    {
        public record GetReaderByIdQuery(Guid Id) : IRequest<Result<GetReaderByIdResponse>>;

        public class GetReaderByIdHandler(
            IReaderRepository readerRepository
            ) : IRequestHandler<GetReaderByIdQuery, Result<GetReaderByIdResponse>>
        {
            public async Task<Result<GetReaderByIdResponse>> Handle(GetReaderByIdQuery request, CancellationToken cancellationToken)
            {
                var reader = await readerRepository.GetByIdAsync(request.Id);

                if (reader == null)
                    return Result<GetReaderByIdResponse>.Failure("Not found bro!");

                return Result<GetReaderByIdResponse>.Success(reader.Adapt<GetReaderByIdResponse>(), "Retrieved");
            }
        }

        public record GetReaderByIdResponse(Guid Id, string Name, string Email, IEnumerable<Subscription> Subscriptions);
    }
}

using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetAllReader
    {
        public record GetAllReaderQuery(int Page) : IRequest<Result<IEnumerable<GetAllReaderResponse>>>;

        public class GetAllReaderHandler(IReaderRepository readerRepository) : IRequestHandler<GetAllReaderQuery, Result<IEnumerable<GetAllReaderResponse>>>
        {
            public async Task<Result<IEnumerable<GetAllReaderResponse>>> Handle(GetAllReaderQuery request, CancellationToken cancellationToken)
            {
                var page = new PageRequest
                {
                    Page = request.Page,
                    PageSize = 10
                };

                var readers = await readerRepository.GetReadersAsync(page, true);

                var readersData = readers.Items.Where(z => !z.IsDeleted)
                    .Select(x => new GetAllReaderResponse(
                        x.Id,
                        x.Name,
                        x.Email,
                        x.Subscriptions.Where(x => !x.IsDeleted)
                        )).ToList();

                return Result<IEnumerable<GetAllReaderResponse>>.Success(readersData, "Retrieved successfully");
            }
        }
        public record GetAllReaderResponse(Guid Id, string Name, string Email, IEnumerable<Subscription> Subscriptions);
    }
}

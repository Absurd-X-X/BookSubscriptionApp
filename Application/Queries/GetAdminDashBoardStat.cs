using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetAdminDashBoardStat
    {
        public record GetAdminDashBoardStatQuery() : IRequest<Result<GetAdminDashBoardStatResponse>>;

        public class GetAdminDashBoardStatHandler(IReaderRepository readerRepository,
            IWalletTransactionRepository transactionRepository,
            ISubscriptionRepository subscriptionRepository,
            IBookRepository bookRepository,
            ILibraryRepository libraryRepository) : IRequestHandler<GetAdminDashBoardStatQuery, Result<GetAdminDashBoardStatResponse>>
        {
            public async Task<Result<GetAdminDashBoardStatResponse>> Handle(GetAdminDashBoardStatQuery request, CancellationToken cancellationToken)
            {
                var readers = await readerRepository.GetReadersAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                }, false);

                var transactions = await transactionRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                }, false);
                var subscriptions = await subscriptionRepository.GetSubscriptionsAsync(false, new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                });
                var books = await bookRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                }, false);
                var libraries = await libraryRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 10,
                }, false);



                return Result<GetAdminDashBoardStatResponse>.Success(new GetAdminDashBoardStatResponse(
                    readers.Items.Count(),
                    transactions.Items.Where(t => t.Status == WalletTransactionStatus.Successful &&
                        t.Type == TransactionType.Credit)
                        .Sum(t => t.Balance),
                    subscriptions.Items.Count(),
                    libraries.Items.Count(),
                    books.Items.Count(),
                    books.Items
                    ), "Retrieved");
            }
        }

        public record GetAdminDashBoardStatResponse(
            int TotalReader,
            decimal TotalRevenue,
            int TotalSubscription,
            int TotalLibrary,
            int BookCount,
            IEnumerable<Book> Books
            );
    }
}

using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetLibraryDashboard
    {
        public record GetLibraryDashboardQuery(Guid LibraryId,
            Guid UserId) : IRequest<Result<GetLibraryDashboardResponse>>;

        public class GetLibraryDashboardHandler(
            IBookRepository bookRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IReviewRepository reviewRepo) : 
                IRequestHandler<GetLibraryDashboardQuery, Result<GetLibraryDashboardResponse>>
        {
            public async Task<Result<GetLibraryDashboardResponse>> Handle(GetLibraryDashboardQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 10
                }, false);

                var user = await userRepository.GetAsync(request.UserId);
                var audits = await auditLogRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 5
                }, false);

                var reviews = await reviewRepo.GetByLibraryIdAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = 5
                }, false, request.LibraryId);

                if (user is null)
                    return Result<GetLibraryDashboardResponse>.Failure("You are not found");

                return Result<GetLibraryDashboardResponse>.Success(new GetLibraryDashboardResponse(
                    books.Items.Count(u => u.LibraryId == request.LibraryId && !u.IsDeleted),

                    user.Wallet!.Balance,

                    books.Items.Where(v => v.LibraryId == request.LibraryId && !v.IsDeleted),

                    books.Items
                    .OrderByDescending(v => v.NoOfTimeReadByPeople)
                    .Where(x => x.LibraryId == request.LibraryId && !x.IsDeleted),

                    books.Items
                    .OrderByDescending(v => v.NoOfTimeReadByPeople)
                    .Where(x => x.LibraryId == request.LibraryId && !x.IsDeleted && x.DateCreated.Month == DateTime.UtcNow.Month),

                    books.Items
                    .OrderByDescending(v => v.NoOfTimeReadByPeople)
                    .Count(x => x.LibraryId == request.LibraryId && !x.IsDeleted && x.NoOfTimeReadByPeople != 0),

                    [.. audits.Items.OrderByDescending(x => x.Timestamp).Take(5)],

                    [.. reviews.Items.OrderByDescending(x => x.DateCreated).Take(5)]

                    ), "Retrived");
            }
        }

        public record GetLibraryDashboardResponse(
            int NoOfBookAdded,
            decimal Balance,
            IEnumerable<Book> AddedBooks,
            IEnumerable<Book> MostRead,
            IEnumerable<Book> BookCreatedThisMonth,
            int TotalNoOfLibraryBooksRead,
            IEnumerable<AuditLog> RecentActivity,
            IEnumerable<Review> RecentReviews
            );
    }
}

// Application.Queries/GetLibraryReviews.cs
using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetLibraryReviews
    {
        public record GetLibraryReviewsQuery(Guid LibraryId, DateTime StartDate, DateTime EndDate, int Page, int PageSize)
            : IRequest<Result<LibraryReviewsResponse>>;

        public class GetLibraryReviewsHandler(IReviewRepository reviewRepository)
            : IRequestHandler<GetLibraryReviewsQuery, Result<LibraryReviewsResponse>>
        {
            public async Task<Result<LibraryReviewsResponse>> Handle(
                GetLibraryReviewsQuery request, CancellationToken cancellationToken)
            {
                var start = request.StartDate;
                var end = request.EndDate;
                var prevStart = start.AddMonths(-1);
                var prevEnd = start.AddDays(-1);

                var reviews = await reviewRepository.GetByLibraryIdAndDateRangeAsync(request.LibraryId, start, end);
                var prevReviews = await reviewRepository.GetByLibraryIdAndDateRangeAsync(request.LibraryId, prevStart, prevEnd);

                // --- Stat cards ---
                var totalReviews = reviews.Count;
                var prevTotalReviews = prevReviews.Count;
                var totalReviewsGrowth = PercentChange(prevTotalReviews, totalReviews);

                var approvedReviews = reviews.Count(r => r.Status == ReviewStatus.Approved);
                var prevApprovedReviews = prevReviews.Count(r => r.Status == ReviewStatus.Approved);
                var approvedReviewsGrowth = PercentChange(prevApprovedReviews, approvedReviews);

                var pendingReviews = reviews.Count(r => r.Status == ReviewStatus.Pending);
                var prevPendingReviews = prevReviews.Count(r => r.Status == ReviewStatus.Pending);
                var pendingReviewsGrowth = PercentChange(prevPendingReviews, pendingReviews);

                var rejectedReviews = reviews.Count(r => r.Status == ReviewStatus.Rejected);
                var prevRejectedReviews = prevReviews.Count(r => r.Status == ReviewStatus.Rejected);
                var rejectedReviewsGrowth = PercentChange(prevRejectedReviews, rejectedReviews);

                var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                var prevAvgRating = prevReviews.Any() ? prevReviews.Average(r => r.Rating) : 0;
                var avgRatingChange = Math.Round((decimal)(avgRating - prevAvgRating), 1);

                // --- Rating distribution ---
                var ratingDist = new RatingDistributionDto(
                    FiveStar: reviews.Count(r => r.Rating == 5),
                    FiveStarPct: PctOfTotal(reviews.Count(r => r.Rating == 5), totalReviews),
                    FourStar: reviews.Count(r => r.Rating == 4),
                    FourStarPct: PctOfTotal(reviews.Count(r => r.Rating == 4), totalReviews),
                    ThreeStar: reviews.Count(r => r.Rating == 3),
                    ThreeStarPct: PctOfTotal(reviews.Count(r => r.Rating == 3), totalReviews),
                    TwoStar: reviews.Count(r => r.Rating == 2),
                    TwoStarPct: PctOfTotal(reviews.Count(r => r.Rating == 2), totalReviews),
                    OneStar: reviews.Count(r => r.Rating == 1),
                    OneStarPct: PctOfTotal(reviews.Count(r => r.Rating == 1), totalReviews)
                );

                // --- Top reviewed books ---
                var topBooks = reviews
                    .GroupBy(r => new { r.BookId, r.Book.Title })
                    .Select(g => new TopReviewedBookDto(
                        g.Key.Title,
                        g.Count(),
                        Math.Round(g.Average(r => r.Rating), 1)))
                    .OrderByDescending(x => x.ReviewCount)
                    .Take(5)
                    .ToList();

                // --- Review rows for table ---
                var reviewRows = reviews
                    .OrderByDescending(r => r.DateCreated)
                    .Select(r => new ReviewRowDto(
                        r.Id,
                        r.Reader.Id,
                        r.Reader.Name,
                        r.Reader.Email,
                        r.BookId,
                        r.Book.Title,
                        r.Book.Author,
                        r.Book.BookCoverUrl,
                        r.Rating,
                        r.Comment,
                        r.Status.ToString(),
                        r.DateCreated
                    ))
                    .ToList();

                var totalCount = reviewRows.Count;
                var currentPage = request.Page < 1 ? 1 : request.Page;
                var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
                var pagedRows = reviewRows
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new LibraryReviewsResponse(
                    PeriodStart: start,
                    PeriodEnd: end,
                    TotalReviews: totalReviews,
                    TotalReviewsGrowthPercent: totalReviewsGrowth,
                    ApprovedReviews: approvedReviews,
                    ApprovedReviewsGrowthPercent: approvedReviewsGrowth,
                    PendingReviews: pendingReviews,
                    PendingReviewsGrowthPercent: pendingReviewsGrowth,
                    RejectedReviews: rejectedReviews,
                    RejectedReviewsGrowthPercent: rejectedReviewsGrowth,
                    AverageRating: Math.Round(avgRating, 1),
                    AverageRatingChange: avgRatingChange,
                    RatingDistribution: ratingDist,
                    TopReviewedBooks: topBooks,
                    Reviews: new PagenatedList<ReviewRowDto>
                    {
                        Items = pagedRows,
                        Page = currentPage,
                        PageSize = pageSize,
                        TotalCount = totalCount
                    }
                );

                return Result<LibraryReviewsResponse>.Success(response, "Success");
            }

            private static int PctOfTotal(int part, int total) =>
                total > 0 ? (int)Math.Round((double)part / total * 100) : 0;

            private static decimal PercentChange(int previous, int current) =>
                previous > 0 ? Math.Round((decimal)(current - previous) / previous * 100, 1) : 0;
        }

        public record LibraryReviewsResponse(
            DateTime PeriodStart,
            DateTime PeriodEnd,
            int TotalReviews,
            decimal TotalReviewsGrowthPercent,
            int ApprovedReviews,
            decimal ApprovedReviewsGrowthPercent,
            int PendingReviews,
            decimal PendingReviewsGrowthPercent,
            int RejectedReviews,
            decimal RejectedReviewsGrowthPercent,
            double AverageRating,
            decimal AverageRatingChange,
            RatingDistributionDto RatingDistribution,
            List<TopReviewedBookDto> TopReviewedBooks,
            PagenatedList<ReviewRowDto> Reviews
        );

        public record RatingDistributionDto(
            int FiveStar, int FiveStarPct,
            int FourStar, int FourStarPct,
            int ThreeStar, int ThreeStarPct,
            int TwoStar, int TwoStarPct,
            int OneStar, int OneStarPct
        );

        public record TopReviewedBookDto(string Title, int ReviewCount, double AverageRating);

        public record ReviewRowDto(
            Guid ReviewId,
            Guid ReaderId,
            string ReviewerName,
            string ReviewerEmail,
            Guid BookId,
            string BookTitle,
            string BookAuthor,
            string? BookCoverUrl,
            int Rating,
            string Comment,
            string Status,
            DateTime DateCreated
        );
    }
}
using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetReviewById
    {
        public record GetReviewByIdQuery(Guid Id) : IRequest<Result<GetReviewByIdResponse>>;

        public class GetReviewByIdHandler(
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetReviewByIdQuery, Result<GetReviewByIdResponse>>
        {
            public async Task<Result<GetReviewByIdResponse>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
            {
                var review = await reviewRepository.GetByIdAsync(request.Id);

                if (review is null)
                {
                    return Result<GetReviewByIdResponse>.Failure("Data not found");
                }

                var reviewerTotalReviews = await reviewRepository.CountByReaderIdAsync(review.ReaderId);
                var bookReviewCount = await reviewRepository.CountByBookIdAsync(review.BookId);
                var bookAvgRating = await reviewRepository.GetAverageRatingForBookAsync(review.BookId);

                var reviewData = new GetReviewByIdResponse(
                    ReviewId: review.Id,
                    Rating: review.Rating,
                    Comment: review.Comment,
                    Status: review.Status,
                    HelpfulCount: review.HelpfulCount,
                    NotHelpfulCount: 0,
                    DateCreated: review.DateCreated,
                    EditedAt: review.EditedAt,

                    BookId: review.Book.Id,
                    BookTitle: review.Book.Title,
                    BookAuthor: review.Book.Author,
                    BookCoverUrl: review.Book.BookCoverUrl,
                    BookCategory: review.Book.Category?.Name ?? "N/A",
                    BookPublicationYear: review.Book.PublicationYear,
                    BookAverageRating: Math.Round(bookAvgRating, 1),
                    BookReviewCount: bookReviewCount,

                    ReaderId: review.Reader.Id,
                    ReaderName: review.Reader.Name,
                    ReaderEmail: review.Reader.Email,
                    ReaderMemberSince: review.Reader.DateCreated,
                    ReaderTotalReviews: reviewerTotalReviews,
                    ReaderLocation: "N/A"
                    );

                return Result<GetReviewByIdResponse>.Success(reviewData, "Retrived");
            }
        }

        public record GetReviewByIdResponse(
        Guid ReviewId,
        int Rating,
        string Comment,
        ReviewStatus Status,
        int HelpfulCount,
        int NotHelpfulCount,
        DateTime DateCreated,
        DateTime? EditedAt,

        Guid BookId,
        string BookTitle,
        string BookAuthor,
        string? BookCoverUrl,
        string BookCategory,
        int BookPublicationYear,
        double BookAverageRating,
        int BookReviewCount,

        Guid ReaderId,
        string ReaderName,
        string ReaderEmail,
        DateTime ReaderMemberSince,
        int ReaderTotalReviews,
        string ReaderLocation
        );
    }
}
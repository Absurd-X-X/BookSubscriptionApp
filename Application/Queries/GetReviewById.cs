using Application.Common.Dtos;
using Application.Common.Repositories;
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

                var reviewData = new GetReviewByIdResponse(
                    review.Id,
                    review.BookId,
                    review.ReaderId,
                    review.Rating,
                    review.Comment,
                    review.IsApproved,
                    review.HelpfulCount
                    );

                return Result<GetReviewByIdResponse>.Success(reviewData, "Retrived");
            }
        }
    }

    public record GetReviewByIdResponse(
        Guid ReviewId,
        Guid BookId,
        Guid ReaderId,
        int Rating,
        string Comment,
        bool IsApproved,
        int HelpfulCount
        );
}


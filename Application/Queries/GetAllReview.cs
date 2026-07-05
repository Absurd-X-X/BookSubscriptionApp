using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllReview
    {
        public record GetAllReviewQuery(int Page, int PageSize) : IRequest<Result<PagenatedList<GetAllReviewResponse>>>;

        public class GetAllReviewHandler(
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetAllReviewQuery, Result<PagenatedList<GetAllReviewResponse>>>
        {
            public async Task<Result<PagenatedList<GetAllReviewResponse>>> Handle(GetAllReviewQuery request, CancellationToken cancellationToken)
            {
                var reviews = await reviewRepository.GetAllAsync(new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                }, true);

                var reviewData = reviews.Items.Select(x => new GetAllReviewResponse(
                    x.Id,
                    x.BookId,
                    x.ReaderId,
                    x.Rating,
                    x.Comment,
                    x.IsApproved,
                    x.HelpfulCount
                    ));

                var pagedDatas = new PagenatedList<GetAllReviewResponse>
                {
                    Items = reviewData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = reviews.TotalCount,
                };

                return Result<PagenatedList<GetAllReviewResponse>>.Success(pagedDatas, "Retrived");
            }
        }

        public record GetAllReviewResponse(
            Guid ReviewId,
            Guid BookId,
            Guid ReaderId,
            int Rating,
            string Comment,
            bool IsApproved,
            int HelpfulCount
            );
    }
}

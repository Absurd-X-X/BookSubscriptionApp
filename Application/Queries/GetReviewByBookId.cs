using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetReviewByBookId
    {
        public record GetReviewByBookIdQuery(Guid Id, int Page, int PageSize) : IRequest<Result<PagenatedList<GetReviewByBookIdResponse>>>;

        public class GetReviewByBookIdHandler(
            IReviewRepository reviewRepository
            ) : IRequestHandler<GetReviewByBookIdQuery, Result<PagenatedList<GetReviewByBookIdResponse>>>
        {
            public async Task<Result<PagenatedList<GetReviewByBookIdResponse>>> Handle(GetReviewByBookIdQuery request, CancellationToken cancellationToken)
            {
                var reviews = await reviewRepository.GetByBookIdAsync(new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                }, true, request.Id);

                var reviewData = reviews.Items.Select(x => new GetReviewByBookIdResponse(
                    x.Id,
                    x.BookId,
                    x.ReaderId,
                    x.Rating,
                    x.Comment,
                    x.IsApproved,
                    x.HelpfulCount
                    ));

                var pagedDatas = new PagenatedList<GetReviewByBookIdResponse>
                {
                    Items = reviewData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = reviews.TotalCount,
                };

                return Result<PagenatedList<GetReviewByBookIdResponse>>.Success(pagedDatas, "Retrived");
            }
        }

        public record GetReviewByBookIdResponse(
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

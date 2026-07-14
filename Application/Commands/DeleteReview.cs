using Application.Common.Repositories;
using MediatR;

namespace Application.Features.ReaderEngagement.Commands.DeleteReview
{
    public sealed record DeleteReviewCommand(Guid ReviewId, Guid ReaderId) : IRequest<bool>;

    public sealed class DeleteReviewCommandHandler(IReviewRepository reviewRepository)
        : IRequestHandler<DeleteReviewCommand, bool>
    {
        public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken ct)
        {
            var review = await reviewRepository.GetByIdAsync(request.ReviewId);
            if (review is null || review.ReaderId != request.ReaderId) return false;

            review.IsDeleted = true;
            return true;
        }
    }
}
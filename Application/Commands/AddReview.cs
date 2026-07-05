using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddReview
    {
        public record AddReviewCommand(int Rating, string Comment) : IRequest<Result<string>>;

        public class AddReviewHandler(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<AddReviewCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddReviewCommand request, CancellationToken cancellationToken)
            {
                await reviewRepository.AddAsync(new Review
                {
                    Comment = request.Comment,
                    Rating = request.Rating
                });

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Created!", "Successfully");
            }
        }
    }
}

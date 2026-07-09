using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class AddReview
    {
        public record AddReviewCommand(int Rating, string Comment, Guid BookId) : IRequest<Result<string>>;

        public class AddReviewHandler(
            IReviewRepository reviewRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<AddReviewCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddReviewCommand request, CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);
                if (book is null)
                    return Result<string>.Failure("The book you wants to add review ");

                Review review = new Review
                {
                    Comment = request.Comment,
                    Rating = request.Rating,
                    BookId = request.BookId
                };
                await reviewRepository.AddAsync(review);



                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Create",
                    Description = $"Notification was archived successfully",
                    Icon = "🔔",
                    IpAddress = ipAddress!,
                    UserRole = book.Library.User.Role,
                    UserId = book.Library.UserId,
                    ResourceType = ResourceType.Review,
                    ResourceId = review.Id
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Created!", "Successfully");
            }
        }
    }
}

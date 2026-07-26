using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class AddReview
    {
        public record AddReviewCommand(int Rating, string Comment, Guid BookId, Guid ReaderId) : IRequest<Result<string>>;


            public class AddReviewHandler(
            IReviewRepository reviewRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IBookRepository bookRepository,
            IReaderRepository readerRepository,   
            IUnitOfWork unitOfWork
            ) : IRequestHandler<AddReviewCommand, Result<string>>
            {
                public async Task<Result<string>> Handle(AddReviewCommand request, CancellationToken cancellationToken)
                {
                    var reader = await readerRepository.GetByIdAsync(request.ReaderId);
                    if (reader is null)
                        return Result<string>.Failure("Reader profile not found.");

                    var book = await bookRepository.GetByIdAsync(request.BookId);
                    if (book is null)
                        return Result<string>.Failure("The book you're trying to review doesn't exist.");

                    Review review = new Review
                    {
                        Comment = request.Comment,
                        Rating = request.Rating,
                        BookId = request.BookId,
                        ReaderId = reader.Id,
                        CreatedBy = reader.User.Id.ToString()
                    };
                    await reviewRepository.AddAsync(review);

                    string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

                    var audit = new AuditLog
                    {
                        ActionType = "Create",
                        Description = $"{reader.Name} reviewed \"{book.Title}\"",
                        Icon = "⭐",
                        IpAddress = ipAddress!,
                        UserRole = reader.User.Role,
                        UserId = reader.UserId,
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

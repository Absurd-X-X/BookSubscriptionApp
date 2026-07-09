using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetNotificationById
    {
        public record GetNotificationByIdQuery(Guid Id) : IRequest<Result<GetNotificationByIdResponse>>;

        public class GetNotificationByIdHandler(
            INotificationRepository notificationRepository,
            IReviewRepository reviewRepository,
            IReaderRepository readerRepository,
            IBookRepository bookRepository,
            ISubscriptionRepository subscriptionRepository,
            IConversationRepository conversationRepository
            ) : IRequestHandler<GetNotificationByIdQuery, Result<GetNotificationByIdResponse>>
        {
            public async Task<Result<GetNotificationByIdResponse>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
            {
                var notification = await notificationRepository.GetById(request.Id);

                if (notification is null)
                    return Result<GetNotificationByIdResponse>.Failure("Not found");

                RelatedReviewDto? relatedReview = null;
                RelatedReaderDto? relatedReader = null;
                RelatedBookDto? relatedBook = null;
                RelatedSubscriptionDto? relatedSubscription = null;
                RelatedChatDto? relatedChat = null;

                if (Guid.TryParse(notification.Ref, out var refId))
                {
                    switch (notification.RefType)
                    {
                        case NotificationRefType.Review:
                            var review = await reviewRepository.GetByIdAsync(refId);
                            if (review is not null)
                            {
                                relatedReview = new RelatedReviewDto(
                                    review.Id,
                                    review.Reader.Name,
                                    review.Reader.Email,
                                    review.Book.Title,
                                    review.Book.Author,
                                    review.Rating,
                                    review.Comment,
                                    review.Status.ToString(),
                                    review.DateCreated,
                                    review.HelpfulCount
                                );
                            }
                            break;

                        case NotificationRefType.Reader:
                            var reader = await readerRepository.GetByIdAsync(refId);
                            if (reader is not null)
                            {
                                relatedReader = new RelatedReaderDto(
                                    reader.Id,
                                    reader.Name,
                                    reader.Email,
                                    reader.DateCreated
                                );
                            }
                            break;

                        case NotificationRefType.Book:
                            var book = await bookRepository.GetByIdAsync(refId);
                            if (book is not null)
                            {
                                relatedBook = new RelatedBookDto(
                                    book.Id,
                                    book.Title,
                                    book.Author,
                                    book.BookCoverUrl,
                                    book.NoOfTimeReadByPeople
                                );
                            }
                            break;

                        case NotificationRefType.WalletTransaction:
                            // Adjust if you have a dedicated repo method — using the same pattern as your revenue dashboard
                            break;

                        case NotificationRefType.Reminder:
                            var subscription = await subscriptionRepository.GetAsync(refId);
                            if (subscription is not null)
                            {
                                relatedSubscription = new RelatedSubscriptionDto(
                                    subscription.Id,
                                    subscription.Types?.ExpiryDate ?? DateTime.MinValue // adjust to real property names
                                );
                            }
                            break;

                        case NotificationRefType.ChatMessage:
                            var conversation = await conversationRepository.GetByIdAsync(refId);
                            if (conversation is not null)
                            {
                                relatedChat = new RelatedChatDto(
                                    conversation.Id,
                                    conversation.Title,
                                    conversation.LastMessageAt
                                );
                            }
                            break;
                    }
                }

                var response = new GetNotificationByIdResponse(
                    Id: notification.Id,
                    Title: notification.Title,
                    Message: notification.Message,
                    Type: notification.Type.ToString(),
                    RefType: notification.RefType.ToString(),
                    IsRead: notification.IsRead,
                    Ref: notification.Ref,
                    DateCreated: notification.DateCreated,
                    RelatedReview: relatedReview,
                    RelatedReader: relatedReader,
                    RelatedBook: relatedBook,
                    RelatedSubscription: relatedSubscription,
                    RelatedChat: relatedChat
                );

                return Result<GetNotificationByIdResponse>.Success(response, "Retrieved");
            }
        }

        public record RelatedReviewDto(Guid ReviewId, string ReviewerName, string ReviewerEmail, string BookTitle, string BookAuthor, int Rating, string Comment, string Status, DateTime SubmittedOn, int HelpfulCount);
        public record RelatedReaderDto(Guid ReaderId, string Name, string Email, DateTime MemberSince);
        public record RelatedBookDto(Guid BookId, string Title, string Author, string? CoverUrl, int TimesRead);
        public record RelatedSubscriptionDto(Guid SubscriptionId, DateTime ExpiryDate);
        public record RelatedChatDto(Guid ConversationId, string Title, DateTime LastMessageAt);

        public record GetNotificationByIdResponse(
            Guid Id, string Title, string Message,
            string Type, string RefType, bool IsRead,
            string Ref, DateTime DateCreated,
            RelatedReviewDto? RelatedReview,
            RelatedReaderDto? RelatedReader,
            RelatedBookDto? RelatedBook,
            RelatedSubscriptionDto? RelatedSubscription,
            RelatedChatDto? RelatedChat
        );
    }
}
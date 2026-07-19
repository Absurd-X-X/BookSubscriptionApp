using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class SendMessage
    {
        public record SendMessageCommand(
            Guid ConversationId,
            Guid SenderId,
            string Content)
            : IRequest<Result<string>>;

        public class SendMessageHandler(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<SendMessageCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                SendMessageCommand request,
                CancellationToken cancellationToken)
            {
                var conversation = await conversationRepository
                    .GetByIdAsync(request.ConversationId);

                if (conversation is null)
                {
                    return Result<string>.Failure("Conversation not found.");
                }

                var isParticipant = conversation.UserConversations
                    .Any(x => x.UserId == request.SenderId);

                if (!isParticipant)
                {
                    return Result<string>.Failure(
                        "You are not a participant in this conversation.");
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return Result<string>.Failure("Message cannot be empty.");
                }

                var message = new Message
                {
                    ConversationId = request.ConversationId,
                    SenderId = request.SenderId,
                    Content = request.Content.Trim(),
                    SentAt = DateTime.UtcNow,
                    CreatedBy = request.SenderId.ToString()
                };

                await messageRepository.AddAsync(message);

                conversation.LastMessageAt = DateTime.UtcNow;

                var receiver = conversation.UserConversations
                    .FirstOrDefault(x => x.UserId != request.SenderId);

                if (receiver != null)
                {
                    await notificationRepository.AddAsync(new Notification
                    {
                        UserId = receiver.UserId,
                        Title = "New Message",
                        RefType = NotificationRefType.ChatMessage,
                        Message = "You have received a new message.",
                        Type = NotificationType.Others,
                        Ref = conversation.Id.ToString(),
                        CreatedBy = request.SenderId.ToString()
                    });
                }

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Message sent successfully.",
                    "Success");
            }
        }
    }
}
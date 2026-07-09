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
                    return Result<string>.Failure(
                        "Conversation not found");

                // Save customer message
                var message = new Message
                {
                    ConversationId = request.ConversationId,
                    SenderId = request.SenderId,
                    Content = request.Content,
                    SentAt = DateTime.UtcNow,
                    CreatedBy = request.SenderId.ToString()
                };

                await messageRepository.AddAsync(message);
                conversation.LastMessageAt = DateTime.UtcNow;

                await unitOfWork.SaveAsync();

                var adminParticipant = conversation.UserConversations
                    .FirstOrDefault(uc => uc.IsAdmin);

                if (adminParticipant is not null)
                {
                    await notificationRepository.AddAsync(new Notification
                    {
                        UserId = adminParticipant.UserId,
                        Title = "New Support Message",
                        RefType = NotificationRefType.ChatMessage,
                        Message = $"New message in: {conversation.Title}",
                        Type = NotificationType.Others,
                        Ref = conversation.Id.ToString(),
                        CreatedBy = request.SenderId.ToString()
                    });

                    await unitOfWork.SaveAsync();
                }

                return Result<string>.Success(
                    "Message sent", "Sharp");
            }
        }
    }
}
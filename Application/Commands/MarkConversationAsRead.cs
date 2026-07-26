using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using MediatR;

namespace Application.Commands
{
    public class MarkConversationAsRead
    {
        public record MarkConversationAsReadCommand(Guid ConversationId, Guid UserId)
            : IRequest<Result<string>>;

        public class MarkConversationAsReadHandler(
            IConversationRepository conversationRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<MarkConversationAsReadCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                MarkConversationAsReadCommand request,
                CancellationToken cancellationToken)
            {
                var conversation = await conversationRepository
                    .GetByIdAsync(request.ConversationId);

                if (conversation is null)
                {
                    return Result<string>.Failure("Conversation not found.");
                }

                var isParticipant = conversation.UserConversations
                    .Any(uc => uc.UserId == request.UserId);

                if (!isParticipant)
                {
                    return Result<string>.Failure(
                        "You are not a participant in this conversation.");
                }

                var unreadMessages = conversation.Messages
                    .Where(m => !m.IsRead
                             && !m.IsDeleted
                             && m.SenderId != request.UserId)
                    .ToList();

                if (unreadMessages.Count == 0)
                {
                    return Result<string>.Success("Nothing to mark.", "Success");
                }

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = DateTime.UtcNow;
                }

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Marked as read.", "Success");
            }
        }
    }
}
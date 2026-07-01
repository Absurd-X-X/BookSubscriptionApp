using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetConversationMessages
    {
        public record GetConversationMessagesQuery(Guid ConversationId)
            : IRequest<Result<List<GetConversationMessagesResponse>>>;

        public class GetConversationMessagesHandler(
            IConversationRepository conversationRepository)
            : IRequestHandler<GetConversationMessagesQuery,
                Result<List<GetConversationMessagesResponse>>>
        {
            public async Task<Result<List<GetConversationMessagesResponse>>> Handle(
                GetConversationMessagesQuery request,
                CancellationToken cancellationToken)
            {
                var conversation = await conversationRepository
                    .GetByIdAsync(request.ConversationId);

                if (conversation is null)
                    return Result<List<GetConversationMessagesResponse>>
                        .Failure("Conversation not found");

                var response = conversation.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new GetConversationMessagesResponse(
                        m.Id,
                        m.SenderId,
                        m.SenderId == Guid.Empty
                            ? "Support Bot"
                            : m.Sender?.UserName ?? "User",
                        m.Content ?? string.Empty,
                        m.SenderId == Guid.Empty,
                        m.IsRead,
                        m.SentAt))
                    .ToList();

                return Result<List<GetConversationMessagesResponse>>
                    .Success(response, "Success");
            }
        }

        public record GetConversationMessagesResponse(
            Guid MessageId,
            Guid SenderId,
            string SenderName,
            string Content,
            bool IsAi,
            bool IsRead,
            DateTime SentAt);
    }
}
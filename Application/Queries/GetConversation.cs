using Application.Common.Dtos;
using Application.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetConversation
    {
        public record GetConversationQuery(Guid Id) : IRequest<Result<GetConversationResponse>>;

        public class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<GetConversationResponse>>
        {
            private readonly IConversationRepository _conversationRepository;
            public GetConversationHandler(IConversationRepository conversationRepository)
            {
                _conversationRepository = conversationRepository;
            }
            public async Task<Result<GetConversationResponse>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
            {
                var conversation = await _conversationRepository.GetByIdAsync(request.Id);
                if (conversation is null) throw new Exception("Conversation not found");

                return Result<GetConversationResponse>.Success(conversation.Adapt<GetConversationResponse>(), "Success!");
            }
        }

        public record GetConversationResponse(Guid Id, string Title, DateTime LastMessageAt);
    }
}